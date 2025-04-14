#include "frog.h"
void helper::FrogClass::auxControllerStop()
{
	//Parada de los motores o del servo.
	if (mvarUsesServo)
	{
		mvarServo->write(mvarServo->read()); //Detenemos el servo.
	}
	else //En caso del motor enviamos un bit bajo a todas las salidas.
	{
		stopMotor();
	}
}
helper::FrogClass::FrogClass()
{
	mvarServo = NULL;
	mvarUsesServo = false;
	mvarHasComprobation = false;
	mvarTravelTime = DEFAULT_TRAVEL_TIME;
	for (uint8_t i = 0; i < 5; i++)
		mcolPort[i] = INVALID_PORT;
}
void helper::FrogClass::init(Servo* servo, uint8_t servoChannel, uint8_t straightAngle, uint8_t curveAngle)
{
	init(servo, servoChannel, straightAngle, curveAngle, INVALID_PORT, INVALID_PORT);
	mvarHasComprobation = false;
}
void helper::FrogClass::init(Servo* servo, uint8_t servoChannel, uint8_t straightAngle, uint8_t curveAngle, uint8_t straightStopPort, uint8_t curveStopPort)
{
	mvarServo = servo;
	mcolPort[0] = servoChannel;
	mcolPort[1] = straightAngle;
	mcolPort[2] = curveAngle;
	mcolPort[3] = straightStopPort;
	mcolPort[4] = curveStopPort;
	setPosition(FrogClass::frogPosition::straight);
	mvarUsesServo = true;
	mvarHasComprobation = true;
}
void helper::FrogClass::init(uint8_t motorEnablePort, uint8_t straightMotorPort, uint8_t curveMotorPort, uint8_t straightStopPort, uint8_t curveStopPort, uint16_t movingDelay)
{
	commonMotorInit(motorEnablePort, straightMotorPort, curveMotorPort, movingDelay);
	mcolPort[3] = straightStopPort;
	mcolPort[4] = curveStopPort;
	mvarHasComprobation = (INVALID_PORT != straightStopPort);
	if (mvarHasComprobation)
	{
		pp(F("Aguja con comprobaci�n")); pl();
	}
	else
	{
		pp(F("Aguja sin comprobaci�n")); pl();
	}
	//setPosition(FrogClass::frogPosition::straight);
}
void helper::FrogClass::init(uint8_t motorEnablePort, uint8_t straightMotorPort, uint8_t curveMotorPort, uint16_t movingDelay)
{
	init(motorEnablePort, straightMotorPort, curveMotorPort, INVALID_PORT, INVALID_PORT, movingDelay);
}
void helper::FrogClass::setPosition(frogPosition rhs)
{
	if (rhs == getTarget()) return; //No tengo que asignar nada si ya est� en la posici�n correcta
	//Esta funci�n pone a trabajar a la aguja. Ya la parar� luego.
	mvarCurrentFrogPosition = frogPosition::indeterminate;
	timerStart();
	mvarTarget = rhs;
	if (mvarUsesServo)
	{
		//En caso de tener servo s�lo tengo que escribir el �ngulo que quiero que tenga.
		//ATENCI�N: El �ngulo que leo de la EEPROM viene dividido por 2 para poder usar un solo byte.
		//En esta parte lo multiplico para obtener el valor original que el usuario guard�.
		uint16_t auxValue;
		if (frogPosition::straight == getTarget())
			auxValue = ((uint16_t)mcolPort[1]) * 2;

		if (frogPosition::curve == getTarget())
			auxValue = ((uint16_t)mcolPort[2]) * 2;

		mvarServo->write(auxValue);
	}
	else
	{
		startMotor(getTarget());
	}
}
bool helper::FrogClass::getCompleted()
{
	//Devuelve True si la aguja est� en la posici�n de destino.
	bool salida = (getTarget() == getPosition());
	if (!mvarUsesServo && salida) stopMotor(); //Ya que no he podido hacer que funcione, parar� los motores en la comprobaci�n.
	return salida;
}
bool helper::FrogClass::getComprobation()
{
	//Si hay comprobaci�n debemos asegurarnos con la lectura del puerto
	if (frogPosition::straight == getTarget())
		return digitalRead(mcolPort[3]);
	else if (frogPosition::curve == getTarget())
		return digitalRead(mcolPort[4]);

	return false;
}
void helper::FrogClass::loop()
{
	//Nada hay que hacer si el desv�o ya est� donde debe.
	if (getCompleted()) return;

	if (mvarUsesServo)
	{
		if (!getCompleted())
		{
			//Comprobaci�n de aguja (si tiene)
			if (mvarHasComprobation)
			{
				if (getComprobation())
				{
					setCompleted();
				}
			}
		}
		if (!getCompleted())
		{
			//Comprobaci�n por posici�n del servo.
			switch (mvarCurrentFrogPosition)
			{
			case helper::FrogClass::straight:
				if (mcolPort[1] == mvarServo->read())
					mvarCurrentFrogPosition = frogPosition::straight;
				break;
			case helper::FrogClass::curve:
				if (mcolPort[2] == mvarServo->read())
					mvarCurrentFrogPosition = frogPosition::curve;
				break;
			}
		}
	}
	else //Motor
	{
		//Se supone que en setPosition() se ha asignado el objetivo
		//Ahora tengo que comprobar si el desv�o ya ha llegado a su objetivo.
			//Comprobaci�n de aguja (si tiene)
		if (mvarHasComprobation)
		{
			if (getComprobation())
			{
				setCompleted();
			}
		}

		if (lapsed()) //Comprobaci�n por difer�metro
		{
			setCompleted();
		}
	}
}
void helper::FrogClass::showConfig()
{
	if (mvarUsesServo)
	{
		pp(F("Servo channel: ")); pp(mcolPort[0]); pl();
		pp(F("Straight angle: ")); pp(((uint16_t)mcolPort[1]) * 2); pl();
		pp(F("Curve angle: ")); pp(((uint16_t)mcolPort[2]) * 2);
	}
	else
	{
		pp(F("Motor enable: ")); pp(mcolPort[0]); pl();
		pp(F("Motor right: ")); pp(mcolPort[1]); pl();
		pp(F("Motor left: ")); pp(mcolPort[2]);
	}
	pl();
	if (mvarHasComprobation)
	{
		pp(F("Straight stop port: ")); pp(mcolPort[3]); pl();
		pp(F("Curve stop port: ")); pp(mcolPort[4]); pl();
	}
	pl();
	pp(F("Maximum travel time: ")); pp(mvarTravelTime); pp(F(" s")); pl();
}
void helper::FrogClass::showStatus()
{
	switch (mvarCurrentFrogPosition)
	{
	case helper::FrogClass::straight:
		pp(F("Straight position (iddle)"));
		if (mvarHasComprobation && getComprobation())
			pp(F(" Comprobated!"));
		break;
	case helper::FrogClass::curve:
		pp("Curve position (iddle)");
		if (mvarHasComprobation && getComprobation())
			pp(F(" Comprobated!"));
		break;
	default:
		if (lapsed())
		{
			pp(F("Error: Lapsed frog"));
		}
		else
		{
			pp(F("Moving to "));
			if (frogPosition::straight == getTarget())
				pp(F(" straight"));
			else if (frogPosition::curve == getTarget())
				pp(F(" curve"));
			else
				pp(F(" indeterminate"));

			if (mvarUsesServo)
			{
				pl();
				pp(F("; current angle is "));
				pp(mvarServo->read());
				pp(F(" and objective angle is "));
				if (frogPosition::straight == getTarget())
					pp(((uint16_t)mcolPort[1]) * 2);
				else if (frogPosition::curve == getTarget())
					pp(((uint16_t)mcolPort[2]) * 2);
				else
					pp(F("indeterminate"));
			}
			pp(F("."));
		}
		break;
		pl();
	}


}

void helper::FrogClass::commonMotorInit(uint8_t motorEnablePort, uint8_t straightMotorPort, uint8_t curveMotorPort, uint16_t movingDelay)
{
	mcolPort[0] = motorEnablePort;
	mcolPort[1] = straightMotorPort;
	mcolPort[2] = curveMotorPort;
	setTravelTime(movingDelay);
	mvarUsesServo = false;
	mvarCurrentFrogPosition = frogPosition::indeterminate; //Ahora no puedo saber en qu� estado est� el desv�o.
}
void helper::FrogClass::startMotor(frogPosition rhs)
{
	if (!digitalRead(mcolPort[0]))
	{
		digitalWrite(mcolPort[0], true);
		digitalWrite(mcolPort[1], (rhs == frogPosition::straight));
		digitalWrite(mcolPort[2], (rhs == frogPosition::curve));
		//pp(F("Motor starting in port ")); pp(mcolPort[0]); pl();
	}
}
void helper::FrogClass::stopMotor()
{
	for (uint8_t i = 0; i < 3; i++)
		digitalWrite(mcolPort[i], false);
	//pp(F("Motor stop in port ")); pp(mcolPort[0]); pl();
}
bool helper::FrogClass::isMotorRunning()
{
	return digitalRead(mcolPort[0]);
}
bool helper::FrogClass::motorRunningDirection()
{
	return digitalRead(mcolPort[1]);
}

