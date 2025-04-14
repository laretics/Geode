#include "MtfInterface.h"
void helper::MtfInterface::Init(uint8_t deviceType)
{
	Timer1.initialize();
	mvarInterfaceType = deviceType;
	mvarResetFirmwareRequested = false;
	//Serial.print(F("Interface init with deviceType="));
	//Serial.print(mvarInterfaceType); Serial.println();
	pinMode(LED_1_PIN, OUTPUT);
	if (DISPLAY_MULTIFUNCTION_SHIELD == mvarInterfaceType)
	{
		MFS.initialize(&Timer1); // inicia la librería.					
		pinMode(LED_2_PIN, OUTPUT);
		pinMode(LED_3_PIN, OUTPUT);
		pinMode(LED_4_PIN, OUTPUT);
		MFS.write("____");
		MFS.blinkDisplay(DIGIT_ALL, OFF);
		queryButton();
		if (mvarLocalMode)
		{
			mvarResetFirmwareRequested = true;
		}
	}
	else
	{
		mvarInterfaceType = DISPLAY_NULL;
		//Hacemos esto por si tenemos la EEPROM con información vieja
		Serial.print(F("No onboard display setup")); Serial.println();
		pinMode(53, INPUT);
		if (HIGH == digitalRead(53))
		{
			Serial.print(F("Hardware reset requested by pin 53")); Serial.println();
			mvarResetFirmwareRequested = true;
		}
	}
}
void helper::MtfInterface::Led(uint8_t id, bool rhs)
{
	if (1 == id)
	{
		digitalWrite(LED_1_PIN, !rhs);
	}
	else if (DISPLAY_MULTIFUNCTION_SHIELD == mvarInterfaceType)
	{
		switch (id)
		{
		case 2:
			digitalWrite(LED_2_PIN, !rhs); break;
		case 3:
			digitalWrite(LED_3_PIN, !rhs); break;
			//case 4:
			//	digitalWrite(LED_4_PIN, !rhs); break;
		default:
			break;
		}
	}
}

void helper::MtfInterface::Sound(uint8_t duration)
{
	if (DISPLAY_MULTIFUNCTION_SHIELD == mvarInterfaceType)
	{
		MFS.beep(duration);
	}
}
void helper::MtfInterface::Sound(uint8_t p1, uint8_t p2, uint8_t p3)
{
	if (DISPLAY_MULTIFUNCTION_SHIELD == mvarInterfaceType)
	{
		MFS.beep(p1, p2, p3);
	}
}
void helper::MtfInterface::loop()
{
	if (!mvarLocalMode)
	{
		if (DISPLAY_MULTIFUNCTION_SHIELD == mvarInterfaceType)
		{
			if (mvarBlinking > 0)
			{
				mvarBlinking--;
			}
			else
			{
				MFS.blinkDisplay(DIGIT_ALL, ON);
			}
		}
	}
	queryButton();
}
void helper::MtfInterface::refreshLocalDisplay(uint8_t deviceId, char* deviceStatus)
{
	if (DISPLAY_MULTIFUNCTION_SHIELD == mvarInterfaceType)
	{
		char auxCadena[8];
		const char* auxCadenaDevice;

		if (helper::deviceType::dtNone == CurrentDeviceType)
		{
			pp("LOCL");
		}
		else if (helper::deviceType::dtSetup == CurrentDeviceType)
		{
			pp("CONF");
		}
		else
		{
			switch (CurrentDeviceType)
			{
			case helper::dtSignal:auxCadenaDevice = "SI"; break;
			case helper::dtLayout:auxCadenaDevice = "PN"; break;
			case helper::dtDetector: auxCadenaDevice = "IN"; break;
			default: auxCadenaDevice = "LO"; break;
			}
			snprintf(auxCadena, sizeof(auxCadena), "%s.%u.%s", auxCadenaDevice, deviceId, deviceStatus);
			pp(auxCadena);
		}
	}
}
void helper::MtfInterface::refreshLocalDisplaynum(uint8_t deviceId, uint8_t deviceStatus)
{
	if (DISPLAY_MULTIFUNCTION_SHIELD == mvarInterfaceType)
	{
		char auxCadena[8];
		const char* auxCadenaDevice;

		if (helper::deviceType::dtNone == CurrentDeviceType)
		{
			pp("LOCL");
		}
		else if (helper::deviceType::dtSetup == CurrentDeviceType)
		{
			pp("CONF");
		}
		else
		{
			switch (CurrentDeviceType)
			{
			case helper::dtSignal:auxCadenaDevice = "SI"; break;
			case helper::dtLayout:auxCadenaDevice = "PN"; break;
			case helper::dtDetector: auxCadenaDevice = "IN"; break;
			default: auxCadenaDevice = "LO"; break;
			}
			snprintf(auxCadena, sizeof(auxCadena), "%s.%u.%u", auxCadenaDevice, deviceId, deviceStatus);
			pp(auxCadena);
		}
	}
}
void helper::MtfInterface::pp(const char* rhs)
{
	if (DISPLAY_MULTIFUNCTION_SHIELD == mvarInterfaceType)
	{
		MFS.write(rhs);
		mvarBlinking = 0xffff;
		MFS.blinkDisplay(DIGIT_ALL, OFF);
	}
	else if (DISPLAY_NULL == mvarInterfaceType)
	{
		Serial.print(F("Disp: "));
		Serial.print(rhs);
		Serial.println();
	}
}

void helper::MtfInterface::queryButton()
{
	uint8_t btn = 0;
	if (DISPLAY_MULTIFUNCTION_SHIELD == mvarInterfaceType)
	{
		btn = MFS.getButton(); // Normally it is sufficient to compare the return
	}
	// value to predefined macros, e.g. BUTTON_1_PRESSED,
	// BUTTON_1_LONG_PRESSED etc.
	if (btn)
	{
		if (mvarLocalMode)
		{
			//Selección de tipo de dispositivo
			if (BUTTON_1_PRESSED == btn)
			{
				if (NULL != ClientObject) ClientObject->HandleButtonPressed(1); //Lo uso sólo para refrescar el display.				
				char auxCadena[7];
				switch (CurrentDeviceType)
				{
				case helper::dtNone:
					CurrentDeviceType = helper::dtSignal;
					break;
				case helper::dtSignal:
					CurrentDeviceType = helper::dtLayout;
					break;
				case helper::dtLayout:
					CurrentDeviceType = helper::dtDetector;
					break;
				case helper::dtDetector:
					CurrentDeviceType = helper::dtSetup;
					break;
				default:
					CurrentDeviceType = helper::dtNone;
					break;
				}
				if (NULL != ClientObject) ClientObject->HandleDeviceTypeSelection();
			}

			if (helper::deviceType::dtNone == CurrentDeviceType)
			{
				//En el modo "None" puedo salir al modo en red.
				if (BUTTON_3_LONG_PRESSED == btn)
				{
					mvarLocalMode = false;
					MFS.write("line");
					MFS.beep(10);
				}
			}

			else if (helper::deviceType::dtSetup == CurrentDeviceType)
			{
				if (BUTTON_1_LONG_PRESSED == btn)
				{
					//Reset
					ClientObject->HandleAdvancedAction(1);
				}
				else if (BUTTON_2_LONG_PRESSED == btn)
				{
					//Formateado
					ClientObject->HandleAdvancedAction(2);
				}
				else if (BUTTON_3_LONG_PRESSED == btn)
				{
					ClientObject->HandleAdvancedAction(3);
					//Otra cosa.
				}
			}
			else
			{
				//Selección de índice
				if (BUTTON_2_PRESSED == btn)
				{
					if (NULL != ClientObject) ClientObject->HandleButtonPressed(2); //Llamo a la rutina que va a gestiona esto.
				}
				//Selección de estado
				if (BUTTON_3_PRESSED == btn)
				{
					if (NULL != ClientObject) ClientObject->HandleButtonPressed(3); //Llamo a la rutina que va a gestiona esto.
				}
			}
		}
		else
		{
			if (btn == BUTTON_1_LONG_PRESSED && !mvarLocalMode)
			{
				mvarLocalMode = true;
				pp("LOCL");
				MFS.beep(1, 1, 10);
				MFS.blinkDisplay(DIGIT_ALL, OFF);
			}
		}
		Led(2, mvarLocalMode);
	}
}