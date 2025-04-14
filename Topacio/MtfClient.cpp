#include "MtfClient.h"

const uint8_t helper::MtfClient::initialFirmwareV1[] = { FIRMWARE_BOOT_FLAG, //Header
											FIRMWARE_VERSION, //Versión
											0, //Display de tipo multifunction
											222,173,190,239,254,0, //MAC
											192,168,0,254, //IP
											24, //Address range
											16, //Structure begin
											16, //First motor address
											0, //Signal count
											0, //Layout units count
											0, //Layout circuits count
}; //FACTORY_ARRAY_LENGTH posiciones de memoria.

helper::MtfClient::MtfClient()
{
	mvarEnabled = false;
}

/// <summary>
/// Deserialización del cliente desde la info almacenada en la Bios.
/// </summary>
void helper::MtfClient::init()
{
	Serial.begin(28800);
	mvarInterface.Init(EEPROM.read(INTERFACE_TYPE_ADDRESS));
	pinMode(A7, INPUT_PULLUP);
	if (digitalRead(A7) == LOW)
	{
		resetFirmwareV1(true);
	}
	else
	{
		pp(F("Montefaro Hardware (c)1989-2025")); pl();
		pp(F("===============================")); pl();

		char auxCadena[5]; //Buffer para cadena de texto.
		snprintf(auxCadena, sizeof(auxCadena), "V%u.%u", FIRMWARE_VERSION / 10, FIRMWARE_VERSION % 10);
		mvarInterface.pp(auxCadena);

		//dummyFirmwareV1();
		if (EEPROM.read(0) == FIRMWARE_BOOT_FLAG)
		{
			uint8_t auxVersion = EEPROM.read(CLIENT_VERSION_ADDRESS);
			if (auxVersion == FIRMWARE_VERSION)
			{
				if (deserializeV1())
				{
					pp(F("Version 1.x deserialized.")); pl();
					mvarInterface.Sound(5);
				}
				else
				{
					pp(F("Boot error: Firmware V1 is corrupted or bad formatted.")); pl();
					mvarInterface.Sound(15);
				}
			}
		}
		else
		{
			pp(F("Boot error: This firmware is corrupted or not initialised.")); pl();
			mvarInterface.Sound(15);
		}
		mvarCurrentLocalDetectorId = 0;
		mvarCurrentLocalLayoutId = 0;
		mvarCurrentLocalSignalId = 0;
		mvarInterface.ClientObject = this;
		mvarEthernet.init();
	}
}

bool helper::MtfClient::deserializeV1()
{
	pp(F("Deserialize V1")); pl();
	mvarInterface.Led(1, true);
	//Lectura de MAC
	memoryDump(32);

	uint16_t auxPointer = CLIENT_ADDRESS_START; //Dirección para leer el MAC
	auxPointer = mvarEthernet.deserializeV1(auxPointer);
	mvarEthernet.showParams();
	mvarInterface.Led(2, true);
	mcolDevices.deserializeV1(auxPointer);
	mvarInterface.Led(2, false);
	showConfig();
	pp(F("Done!")); pl();
	mvarEnabled = true;
	return true;
}

void helper::MtfClient::loop()
{
	//Bucle de repetición
	if (manageSerial()) return;
	uint16_t datos = 0;
	mvarInterface.loop();
	if (mvarInterface.LocalMode())
	{
		mvarEthernet.flush();
	}
	else
	{
		datos = mvarEthernet.loopUDP();
		if (ETHERNET_ERROR == datos)
		{
			mvarInterface.Led(3, true);
		}
		else if (datos > 0)
		{
			mvarInterface.Led(1, true);
			pp(F("Processing UDP Request")); pl();
			processUDPRequest(mvarEthernet.getBuffer(), datos);
			mvarInterface.Led(1, false);
		}
		datos = mvarEthernet.loopTCP();
		if (ETHERNET_ERROR == datos)
		{
			mvarInterface.Led(3, true);
		}
		else if (datos > 0)
		{
			mvarInterface.Led(2, true);
			pp(F("Processing TCP Request")); pl();
			processTCPRequest(mvarEthernet.getBuffer(), datos);
			mvarInterface.Led(2, false);
		}
	}
	mcolDevices.loop();

	mvarInterface.Led(1, false);
}

void helper::MtfClient::HandleDeviceTypeSelection()
{
	//Llamo a esta función cuando he cambiado el tipo de dispositivo.
	refreshLocal();
}
void helper::MtfClient::HandleButtonPressed(uint8_t buttonId)
{
	switch (buttonId)
	{
	case 2:	//Cambio de dispositivo
		switch (mvarInterface.CurrentDeviceType)
		{
		case helper::dtSignal:
			mvarCurrentLocalSignalId++;
			if (mvarCurrentLocalSignalId >= mcolDevices.SignalCount())
				mvarCurrentLocalSignalId = 0;
			break;
		case helper::dtLayout:
			mvarCurrentLocalLayoutId++;
			if (mvarCurrentLocalLayoutId >= mcolDevices.LayoutCount())
				mvarCurrentLocalLayoutId = 0;
		case helper::dtDetector:
			mvarCurrentLocalDetectorId++;
			if (mvarCurrentLocalDetectorId >= mcolDevices.DetectorCount())
				mvarCurrentLocalDetectorId = 0;
		default:
			break;
		}
		break;
	case 3: //Cambio de valor
		uint8_t auxValor;
		switch (mvarInterface.CurrentDeviceType)
		{
		case helper::dtSignal:
			auxValor = (uint8_t)mcolDevices.getSignalOrderByAddress(mvarCurrentLocalSignalId);
			auxValor++;
			if (auxValor > 4) { auxValor = 0; }
			//pp(F("auxValor = ")); pp(auxValor); pl();
			mcolDevices.setSignalOrderByAddress(mvarCurrentLocalSignalId, (helper::orderType)auxValor);
			break;
		case helper::dtLayout:
			auxValor = mcolDevices.getLayout(mvarCurrentLocalLayoutId);
			auxValor++;
			//pp(F("Max layout count: ")); pp(mcolDevices.getLayoutMax(mvarCurrentLocalLayoutId)); pl();
			if (auxValor >= mcolDevices.getLayoutMax(mvarCurrentLocalLayoutId))
				auxValor = 0;
			mcolDevices.setLayoutByAddress(mvarCurrentLocalLayoutId, auxValor);
			break;
		default:
			break;
		}
		break;
	default:
		break;
	}
	refreshLocal();
}
void helper::MtfClient::HandleAdvancedAction(uint8_t actionId)
{
	switch (actionId)
	{
	case 1: //Reset
		break;
	case 2: //Format con máscara de 32 bits
		resetFirmwareV1(true);
		mvarInterface.refreshLocalDisplay(0, "R_16");
		break;
	case 3: //Format con máscara de 16 bits
		resetFirmwareV1(false);
		mvarInterface.refreshLocalDisplay(0, "R_32");
		break;
	default:
		break;
	}
}
void helper::MtfClient::refreshLocal()
{
	//Tengo que cargar los datos actuales para mostrar
	switch (mvarInterface.CurrentDeviceType)
	{
	case helper::dtSignal:
		refreshCurrentSignalToInterface(); break;
	case helper::dtLayout:
		refreshCurrentLayoutToInterface(); break;
	case helper::dtDetector:
		refreshCurrentDetectorToInterface(); break;
	case helper::dtSetup:
		refreshSetupModeToInterface(); break;
	default:
		mvarInterface.refreshLocalDisplay(0, NULL); break;
	}
}
void helper::MtfClient::refreshCurrentSignalToInterface()
{
	helper::orderType auxOrder = mcolDevices.getSignalOrderByAddress(mvarCurrentLocalSignalId);
	char* auxtext;
	switch (auxOrder)
	{
	case helper::orderType::toViaLibre: auxtext = "L"; break;
	case helper::orderType::toAvisoDeParada: auxtext = "A"; break;
	case helper::orderType::toPrecaucion: auxtext = "P"; break;
	case helper::orderType::toRebaseAutorizado: auxtext = "b";	break;
	default: auxtext = "R"; break;//Parada
	}
	mvarInterface.refreshLocalDisplay(mvarCurrentLocalSignalId, auxtext);
}
void helper::MtfClient::refreshCurrentLayoutToInterface()
{
	mvarInterface.refreshLocalDisplaynum(mvarCurrentLocalLayoutId, mcolDevices.getLayout(mvarCurrentLocalLayoutId));
}
void helper::MtfClient::refreshCurrentDetectorToInterface()
{
	if (mcolDevices.getDetector(mvarCurrentLocalDetectorId))
	{
		mvarInterface.refreshLocalDisplay(mvarCurrentLocalDetectorId, "O");
	}
	else
	{
		mvarInterface.refreshLocalDisplay(mvarCurrentLocalDetectorId, "F");
	}
}

void helper::MtfClient::refreshSetupModeToInterface()
{
	mvarInterface.refreshLocalDisplay(0, NULL);
}

void helper::MtfClient::showConfig()
{
	//Muestra la configuración de todos los elementos instalados.
	pp(F("Client configuration:")); pl();
	pp(F("=====================")); pl();

	//Versión del firmware:
	uint8_t auxVersion = EEPROM.read(1);
	pp(F("Firmware Version: ")); pp(auxVersion); pl();

	mvarEthernet.showParams();
	pl();
	pl();
	mcolDevices.showConfig();
}

/// <summary>
/// Carga el firmware de la versión 1 vacío para iniciar correctamente el módulo.
/// Mask16: Parámetro boolean. Si es true, se reestablece con una dirección con máscara 255.255.0.0
/// En otro caso, la máscara será 255.255.255.0
/// </summary>
void helper::MtfClient::resetFirmwareV1(bool mask16)
{
	pp(F("Restoring factory Bios (V1)")); pl();
	uint8_t auxCRC = 0;
	for (uint16_t i = 0; i < FACTORY_ARRAY_LENGTH; i++)
	{
		if (mask16 && 13 == i)
		{
			EEPROM.write(i, 16);//Reset con máscara de 16 bits.
			auxCRC ^= 16;
		}
		else
		{
			EEPROM.write(i, initialFirmwareV1[i]);
			auxCRC ^= initialFirmwareV1[i];
		}
	}
	EEPROM.write(FACTORY_ARRAY_LENGTH, auxCRC); //Añadimos el CRC a mano para no tener que calcular.
	mvarInterface.Sound(3, 2, 40);
	mvarInterface.Led(1, true);
	mvarInterface.Led(2, true);
	mvarInterface.Led(3, true);
	pp(F("Bios rewritten. Reset controller!")); pl();
}

bool helper::MtfClient::manageSerial()
{
	if (Serial.available() > 0) {
		char auxCaracter = Serial.read();
		if ('f' == auxCaracter)
		{
			resetFirmwareV1(false);
		}
		else
		{
			pp("Unknown command");
		}
		pl();
	}
	return false; //No ha leído ningún comando
}

void helper::MtfClient::memoryDump(uint8_t length)
{
	pp(F("Address\tValue")); pl();
	for (uint8_t i = 0; i < length; i++)
	{
		pp(i);
		pt();
		pp(EEPROM.read(i));
		pl();
	}
}

/// <summary>
/// Procesa una petición de Ethernet.
/// </summary>
/// <param name="client">Referencia al cliente que ha emitido el mensaje</param>
/// <param name="bufferLength">Longitud de la información en el buffer</param>
/// <returns>True si ha conseguido procesar correctamente la petición</returns>
bool helper::MtfClient::processTCPRequest(uint8_t* buffer, uint16_t length)
{
	mvarInterface.Led(3, false); //Apagamos la visualización de error temporalmente
	switch (buffer[3]) //Parse de la instrucción
	{
	case 1:	//Firmware update
		return updateFirmwareFromBuffer(buffer, length);
		break;

	default:
		mvarInterface.Led(3, true); //Led de error.
		pp(F("Unknown TCP Command (")); pp(buffer[3]); pp(F(")!!")); pl();
		return false;
		break;
	}
}
bool helper::MtfClient::processUDPRequest(uint8_t* buffer, uint16_t length)
{
	mvarInterface.Led(3, false); //Apagamos la visualización de error temporalmente
	//Escribimos en el display los minutos y los segundos del envío...
	char auxCadena[6]; //Buffer para cadena de texto.
	snprintf(auxCadena, sizeof(auxCadena), "%02d.%02d", buffer[3], buffer[4]);
	mvarInterface.pp(auxCadena);
	//MFS.write(auxCadena);
	switch (buffer[5]) //Parse de la instrucción
	{
	case 1: //Actualización de señales y circuitos
		//return setClientsFromBuffer(); break;
		bool salida = mcolDevices.processBuffer(buffer);
		if (!salida) mvarInterface.Led(3, true);//Led de error.
		return salida; break;
	default:
		pp(F("Unknown UDP message type")); pl();
		return false;
		break;
	}
}
bool helper::MtfClient::updateFirmwareFromBuffer(uint8_t* buffer, uint16_t length)
{
	pp(F("Firmware updating process")); pl();
	uint16_t auxAmount = buffer[4] + (buffer[5] * 256);
	if (auxAmount > ETHERNET_BUFFER_LENGTH - 6 || auxAmount < 10)
	{
		mvarInterface.Led(3, true);
		pp(F("Error: There are ")); pp(auxAmount); pp(F(" bytes to update. This amount is not legal.")); pl();
		mvarEthernet.writeResponse(5); //Error de datos ilegales
		mvarInterface.Sound(100, 10, 5);
		return false;
	}
	if (buffer[7] > FIRMWARE_VERSION) //Comprobación de versión
	{
		mvarInterface.Led(3, true); //Led de error.
		pp(F("Firmware version Error: Expected till ")); pp(FIRMWARE_VERSION); pp(F(" and found V")); pp(buffer[6]); pl();
		mvarEthernet.writeResponse(2);
		mvarInterface.Sound(100, 10, 2);
		return false;
	}
	uint16_t auxPointer = 6;
	mvarInterface.Led(2, true); //Led de escritura en EEPROM
	//Comprobación de CRC
	uint8_t auxCRC = 0;
	for (uint16_t i = 0; i < auxAmount - 1; i++)
	{
		auxCRC ^= buffer[auxPointer++];
	}
	if (auxCRC != buffer[auxPointer])
	{
		mvarInterface.Led(2, false); //Fin de actualización.
		mvarInterface.Led(3, true); //Led de error.
		pp(F("CRC Error: Expected ")); pp(auxCRC); pp(F(" and found ")); pp(buffer[auxPointer]); pl();
		mvarEthernet.writeResponse(1);
		mvarInterface.Sound(100);
		return false;
	}
	auxPointer = 6;
	for (uint16_t i = 0; i < auxAmount; i++)
	{
		EEPROM.write(i, buffer[auxPointer++]);
	}
	mvarInterface.Led(2, false);
	pp(auxAmount); pp(F(" bytes wrote in EEPROM. Please reboot controller to load new firmware."));
	mvarEthernet.writeResponse(0);
	mvarInterface.Sound(10, 5, 10);
	return true;
}
