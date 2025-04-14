#include "MtfClientDevices.h"
helper::MtfClientDevices::MtfClientDevices()
{

}

bool helper::MtfClientDevices::deserializeV1(uint16_t pointer)
{
	pp(F("Leyendo address ")); pp(pointer); pl();
	uint16_t auxDirComienzo = EEPROM.read(pointer++); //Esta es la dirección de memoria donde comienza a leer la estructura
	pp(F("Comienzo estructura: ")); pp(auxDirComienzo); pl();
	uint8_t auxMotorPointer = EEPROM.read(pointer++); //Dirección del primer motor
	uint16_t auxServoPointer = pointer;//Dirección del primer servo.
	//Sección de servos. La saltamos para ir directamente a la dirección de comienzo.
	pointer = auxDirComienzo;
	uint8_t auxSignalAmount = EEPROM.read(pointer++); //Número de señales.
	pp(F("Número de senales: ")); pp(auxSignalAmount); pl();
	uint8_t auxLayoutAmount = EEPROM.read(pointer++); //Número de secciones de enclavamiento.
	uint8_t auxDetectorAmount = EEPROM.read(pointer++); //Número de circuitos de vía (o detectores de presencia).
	pp(F("Reading ")); pp(auxSignalAmount); pp(F(" signals.")); pl();
	for (int signalId = 0; signalId < auxSignalAmount; signalId++)
	{
		pointer = deserializeSignal(pointer);
	}
	pp(F("Reading ")); pp(auxLayoutAmount); pp(F(" layout sections.")); pl();
	for (int layoutId = 0; layoutId < auxLayoutAmount; layoutId++)
	{
		pointer = deserializeLayoutUnit(pointer, auxMotorPointer);
	}
	pp(F("Reading ")); 	pp(auxLayoutAmount); pp(F(" layout circuits.")); pl();
	for (int circuitId = 0; circuitId < auxDetectorAmount; circuitId++)
	{
		pointer = deserializeDetector(pointer);
	}
}
/// <summary>
/// Deserializa una señal de la estructura
/// </summary>
/// <param name="pointer">Puntero que apunta hacia esta señal</param>
/// <returns>Puntero que apunta hacia el siguiente objeto a deserializar</returns>
uint16_t helper::MtfClientDevices::deserializeSignal(uint16_t pointer)
{
	//Deserialización de una señal de tráfico
	uint16_t auxPointer = pointer;
	uint8_t signalRefId = EEPROM.read(auxPointer++);
	bool tomeuMode = bit_check(signalRefId, 7); //Extraigo el flag del modo Tomeu del Id de la señal.
	signalRefId = bit_set(signalRefId, 7, false); //Quito el flag del número para conservar su denominación.
	uint8_t servoAddress = EEPROM.read(auxPointer++);
	uint8_t greenLedPort = NULL_CHANNEL; bool greenInverted = false;
	uint8_t redLedPort = NULL_CHANNEL; bool redInverted = false;
	uint8_t yellowLedPort = NULL_CHANNEL; bool yellowInverted = false;
	uint8_t whiteLedPort = NULL_CHANNEL; bool whiteInverted = false;
	uint8_t lightsCount = EEPROM.read(auxPointer++);
	if (lightsCount > 0)
	{
		greenLedPort = EEPROM.read(auxPointer++);
		if (lightsCount > 1)
		{
			redLedPort = EEPROM.read(auxPointer++);
			if (lightsCount > 2)
			{
				yellowLedPort = EEPROM.read(auxPointer++);
				if (lightsCount > 3)
				{
					whiteLedPort = EEPROM.read(auxPointer++);
				}
			}
		}
		greenInverted = bit_check(greenLedPort, 7);
		greenLedPort = bit_set(greenLedPort, 7, false);
		redInverted = bit_check(redLedPort, 7);
		redLedPort = bit_set(redLedPort, 7, false);
		yellowInverted = bit_check(yellowLedPort, 7);
		yellowLedPort = bit_set(yellowLedPort, 7, false);
		whiteInverted = bit_check(whiteLedPort, 7);
		whiteLedPort = bit_set(whiteLedPort, 7, false);
	}
	if (mvarSignalClientCount < MAX_OF_SIGNALS)
	{
		signalClient* nueva = &mcolSignalClient[mvarSignalClientCount++];
		uint8_t servoPort = NULL_CHANNEL;
		if (NULL_CHANNEL != servoAddress)
		{
			//Esto es un semáforo, con servo.
			Servo* auxServo = &mcolServo[mvarServoCount++];
			servoPort = EEPROM.read(servoAddress) + A0;
			auxServo->attach(servoPort);
			//Cambio importante: Ahora almaceno la mitad del valor de los ángulos para abarcar los 360 grados.
			uint8_t auxValue = (EEPROM.read(servoAddress + 1));
			uint16_t stopAngle = (uint16_t)auxValue * 2;
			auxValue = (EEPROM.read(servoAddress + 2));
			uint16_t allowAngle = (uint16_t)auxValue * 2;
			uint16_t midAngle = (uint8_t)(((int)stopAngle + (int)allowAngle) / 2);
			uint8_t flags = EEPROM.read(servoAddress + 3);
			bool auxTomeu = bit_check(flags, 0);
			bool auxDynamic = bit_check(flags, 1);
			bool speed0 = bit_check(flags, 2);
			bool speed1 = bit_check(flags, 3);

			nueva->setServo(auxServo, servoPort, stopAngle, midAngle, allowAngle, auxTomeu, auxDynamic, speed0, speed1);
		}
		nueva->setLedPort(greenLedPort, redLedPort, yellowLedPort, whiteLedPort, servoPort);
		nueva->setInverted(greenInverted, redInverted, yellowInverted, whiteInverted);
		nueva->init(signalRefId);
	}
	return auxPointer;
}

/// <summary>
/// Extrae un bloque del enclavamiento desde la EEPROM
/// </summary>
/// <param name="pointer">puntero actual de la EEPROM</param>
/// <returns>Puntero al siguiente objeto</returns>
uint16_t helper::MtfClientDevices::deserializeLayoutUnit(uint16_t pointer, uint8_t motorPointer)
{
	uint16_t auxPointer = pointer;
	uint8_t deviceIndex = EEPROM.read(auxPointer++);
	uint8_t frogsCount = EEPROM.read(auxPointer++);
	bool auxSequential = bit_check(frogsCount, 7);
	frogsCount = bit_set(frogsCount, 7, false);
	uint8_t itinerariesCount = EEPROM.read(auxPointer++);
	circuitLayoutClient* auxClienteCir = addLayout(deviceIndex);
	auxClienteCir->setSequential(auxSequential);
	for (uint8_t i = 0; i < frogsCount; i++)
	{
		uint8_t frogAddress = EEPROM.read(auxPointer++);
		uint8_t frogIndex = frogAddress;
		uint16_t auxTimeout = EEPROM.read(auxPointer++);
		auxTimeout |= EEPROM.read(auxPointer++) * 0x100;
		uint8_t auxStraightDetectorPort = EEPROM.read(auxPointer++);
		uint8_t auxCurveDetectorPort = EEPROM.read(auxPointer++);
		uint8_t devicePort = EEPROM.read(frogIndex++);
		uint8_t param0 = EEPROM.read(frogIndex++);
		uint8_t param1 = EEPROM.read(frogIndex++);
		FrogClass* auxFrog = addFrog();
		if (frogAddress < motorPointer)
		{
			//Frog con Servo
			Servo* auxServo = &mcolServo[mvarServoCount++];
			auxServo->attach(devicePort + A0);
			auxFrog->init(auxServo, devicePort, param0, param1, auxStraightDetectorPort, auxCurveDetectorPort);
		}
		else
		{
			//Frog con LM295
			if (INVALID_PORT == auxStraightDetectorPort)
				auxFrog->init(devicePort, param0, param1, auxTimeout);
			else
				auxFrog->init(devicePort, param0, param1, auxStraightDetectorPort, auxCurveDetectorPort, auxTimeout);
		}
		auxClienteCir->addFrog(auxFrog);
	}
	for (uint8_t i = 0; i < itinerariesCount; i++)
	{
		uint8_t auxCommandsCount = EEPROM.read(auxPointer++);
		for (uint8_t f = 0; f < auxCommandsCount; f++)
		{
			uint8_t auxCombined = EEPROM.read(auxPointer++);
			bool auxStraight = bit_check(auxCombined, 7);
			uint8_t auxFrogId = bit_set(auxCombined, 7, false);
			auxClienteCir->setLayoutFrog(i, auxFrogId, auxStraight); //Programamos la posición de la aguja en cada combinación.
		}
	}
	return auxPointer;
}
/// <summary>
/// Extrae un detector de presencia de trenes desde la EEPROM
/// </summary>
/// <param name="pointer">puntero actual de la EEPROM</param>
/// <returns>Puntero al siguiente objeto</returns>
uint16_t helper::MtfClientDevices::deserializeDetector(uint16_t pointer)
{
	uint16_t auxPointer = pointer;
	uint8_t auxId = EEPROM.read(auxPointer++);
	uint8_t auxPort = EEPROM.read(auxPointer++);
	uint8_t auxDelay = EEPROM.read(auxPointer++);
	addDetector(auxId, auxPort, auxDelay);
	return auxPointer;
}

void helper::MtfClientDevices::loop()
{
	for (uint8_t i = 0; i < mvarSignalClientCount; i++)
		mcolSignalClient[i].loop();
	for (uint8_t i = 0; i < mvarCircuitDetectorCount; i++)
		mcolDetectors[i].loop();
	for (uint8_t i = 0; i < mvarCircuitLayoutCount; i++)
		mcolLayouts[i].loop();
}

helper::circuitLayoutClient* helper::MtfClientDevices::addLayout(uint8_t circuitId)
{
	circuitLayoutClient* salida = NULL;
	if (mvarCircuitLayoutCount < MAX_OF_CIRCUITS)
	{
		salida = &mcolLayouts[mvarCircuitLayoutCount];
		mvarCircuitLayoutCount++;
		salida->init(circuitId);
		return salida;
	}
	pp(F("ERROR: Circuit layouts runout!")); pl();
	return NULL;
}
helper::circuitDetectorClient* helper::MtfClientDevices::addDetector(uint8_t circuitId, uint8_t port, uint8_t occupancyDelay)
{
	circuitDetectorClient* salida = NULL;
	if (mvarCircuitDetectorCount < MAX_OF_CIRCUITS)
	{
		salida = &mcolDetectors[mvarCircuitDetectorCount++];
		salida->init(circuitId, port);
		salida->setOccupancyDelay(occupancyDelay);
		return salida;
	}
	pp(F("ERROR: Circuit detectors runout!")); pl();
	return NULL;
}

bool helper::MtfClientDevices::processBuffer(byte* rhs)
{
	uint16_t auxSignalCount = rhs[6];
	uint16_t auxLayoutCount = rhs[7];
	uint16_t auxPointer = 8;
	//pp(auxSignalCount); pp(F(" senales y "));
	//pp(auxLayoutCount); pp(F(" circuitos.")); pl();
	for (uint16_t i = 0; i < auxSignalCount; i++)
	{
		if (auxPointer >= ETHERNET_BUFFER_LENGTH)
		{
			pp(F("Ethernet buffer overload")); pl();
			return false;
		}
		else
		{
			uint8_t auxSignalId = rhs[auxPointer++];
			uint8_t auxSignalValue = rhs[auxPointer++];
			setSignalOrderByIndex(auxSignalId, (helper::orderType)auxSignalValue);
		}
	}
	for (uint16_t i = 0; i < auxLayoutCount; i++)
	{
		if (auxPointer >= ETHERNET_BUFFER_LENGTH)
		{
			pp(F("Ethernet buffer overload")); pl();
			return false;
		}
		else
		{
			uint8_t auxLayoutId = rhs[auxPointer++];
			uint8_t auxLayoutValue = rhs[auxPointer++];
			pp(F("setLayoutByIndex(")); pp(auxLayoutId);
			pp(F(",")); pp(auxLayoutValue);
			pp(F(");")); pl();
			setLayoutByIndex(auxLayoutId, auxLayoutValue);
		}
	}
	return true;
}

void helper::MtfClientDevices::setSignalOrderByIndex(uint8_t id, helper::orderType value)
{
	//Primero buscamos la señal en la base de datos.
	for (int i = 0; i < mvarSignalClientCount; i++)
	{
		if (mcolSignalClient[i].id() == id)
		{
			setSignalOrderByAddress(i, value);
		}
	}
}
void helper::MtfClientDevices::setSignalOrderByAddress(uint8_t address, helper::orderType value)
{
	if (mcolSignalClient[address].getOrder() != value)
	{
		mcolSignalClient[address].setOrder(value);
		//pp(F("Signal ")); pp(address); pp(F(" changed to order ")); pp((uint8_t)value); pl();
	}
}
helper::orderType helper::MtfClientDevices::getSignalOrderByIndex(uint8_t id)
{
	for (int i = 0; i < mvarSignalClientCount; i++)
	{
		if (mcolSignalClient[i].id() == id)
		{
			return getSignalOrderByAddress(i);
		}
	}
	return helper::orderType::toParada; //Retorno por defecto
}
void helper::MtfClientDevices::setLayoutByIndex(uint8_t index, uint8_t value)
{
	//Primero buscamos el circuito en la base de datos
	for (int i = 0; i < mvarCircuitLayoutCount; i++)
	{
		if (mcolLayouts[i].id() == index)
		{
			setLayoutByAddress(i, value);
		}
	}
}
void helper::MtfClientDevices::setLayoutByAddress(uint8_t address, uint8_t value)
{
	if (mcolLayouts[address].getLayout() != value)
	{
		mcolLayouts[address].setLayout(value);
		pp(F("Layout ")); pp(mcolLayouts[address].id()); pp(F(" changed to itinerary ")); pp(value); pl();
	}
}

helper::FrogClass* helper::MtfClientDevices::addFrog() //Obtiene referencia a uno de los mecanismos de control de agujas disponible
{
	if (mvarFrogCount < MAX_OF_FROGS)
	{
		FrogClass* salida = &mcolFrogs[mvarFrogCount++];
		return salida;
	}
	pp(F("ERROR: Frogs runout!")); pl();
	return NULL; //Nos hemos quedado sin mecanismos para agujas en la placa.
}

void helper::MtfClientDevices::showConfig()
{
	pp(F("This client has ")); pp(mvarSignalClientCount);
	pp(F(" signals, ")); pp(mvarCircuitDetectorCount);
	pp(F(" train detectors, ")); pp(mvarCircuitLayoutCount);
	pp(F(" track point controllers and ")); pp(mvarServoCount);
	pp(F(" servos.")); pl();
	pl();
	pp(F("This is the detailed description:")); pl();
	if (mvarSignalClientCount > 0)
	{
		pp(F("Signals and semaphores:")); pl();
		for (uint8_t i = 0; i < mvarSignalClientCount; i++)
			mcolSignalClient[i].showConfig();
		pl();
	}
	if (mvarCircuitLayoutCount > 0)
	{
		pp(F("Track point controllers:")); pl();
		for (uint8_t i = 0; i < mvarCircuitLayoutCount; i++)
			mcolLayouts[i].showConfig();
		pl();
	}
	if (mvarCircuitDetectorCount > 0)
	{
		pp(F("Train detectors:")); pl();
		for (uint8_t i = 0; i < mvarCircuitDetectorCount; i++)
			mcolDetectors[i].showConfig();
		pl();
	}
}