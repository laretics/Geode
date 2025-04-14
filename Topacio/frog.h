//Esto es un modelo para un motor de aguja, que puede tener tambi�n comprobador de posici�n.
#ifndef _FROG_h
#define _FROG_h
#include "helper.h"
#include <Servo.h>
#define INVALID_PORT 0xff
#define DEFAULT_TRAVEL_TIME 1500
namespace helper
{
	class FrogClass
	{
	public:
		enum frogPosition {
			straight, //V�a directa
			curve, //V�a desviada
			indeterminate //Ni v�a directa, ni v�a desviada
		};
	private:
		uint64_t mvarStartTime; //Tiempo en que comenz� a moverse el motor
		bool mvarUsesServo; //Flag que se establece al arrancar, que indica si vamos con servo o con controlador.
		bool mvarHasComprobation; //Flag que se establece al arrancar, que indica si esta aguja tiene comprobaci�n.
		frogPosition mvarCurrentFrogPosition; //Valor que devuelve getPosition();
		void auxControllerStop(); //Para el movimiento del motor.

	protected:
		uint16_t mvarTravelTime; //Tiempo de movimiento en milisegundos (para obtener comprobaci�n)
		Servo* mvarServo; //Motor controlador (si la aguja va por servo)
		uint8_t mcolPort[5]; //Puertos del motor.	
		//Configuraci�n con servo:
		//mcolPort[0] --> puerto del servo.
		//mcolPort[1] --> �ngulo de servo para v�a directa.
		//mcolPort[2] --> �ngulo de servo para v�a desviada.
		//mcolPort[3] --> Fin de carrera directa (si hay)
		//mcolPort[4] --> Fin de carrera desviada (si hay)

		//Configuraci�n con controlador:
		//mcolPort[0] --> Motor enabled 
		//mcolPort[1] --> Motor right
		//mcolPort[2] --> Motor left
		//mcolPort[3] --> Fin de carrera directa (si hay)
		//mcolPort[4] --> Fin de carrera desviada (si hay)
		frogPosition mvarTarget; //Destino del desv�o

	public:
		FrogClass(); //Constructor
		void init(Servo* servo, uint8_t servoChannel, uint8_t straightAngle, uint8_t curveAngle); //Asigna los recursos a este desv�o.
		void init(Servo* servo, uint8_t servoChannel, uint8_t straightAngle, uint8_t curveAngle, uint8_t straightStopPort, uint8_t curveStopPort); //Asigna los recursos a este desv�o.
		void init(uint8_t motorEnablePort, uint8_t straightMotorPort, uint8_t curveMotorPort, uint16_t movingDelay); //Asigna los recursos a este desv�o.
		void init(uint8_t motorEnablePort, uint8_t straightMotorPort, uint8_t curveMotorPort, uint8_t straightStopPort, uint8_t curveStopPort, uint16_t movingDelay); //Asigna los recursos a este desv�o.
		void setPosition(frogPosition rhs); //Asigna la posici�n de este desv�o.
		inline frogPosition getPosition() { return mvarCurrentFrogPosition; } //Obtiene la posici�n actual de este desv�o.
		inline frogPosition getTarget() { return mvarTarget; } //Obtiene el objetivo del desv�o.
		bool getCompleted(); //Indica si la posici�n coincide con la �ltima orden cursada.
		inline void setCompleted() { mvarCurrentFrogPosition = getTarget(); } //Asigna la posici�n terminada.
		inline void setTravelTime(uint16_t rhs) { mvarTravelTime = rhs; }
		bool getComprobation(); //Indica si el desv�o ha comprobado que la �ltima posici�n es correcta.
		inline void timerStart() { mvarStartTime = millis(); } //Pone a contar el difer�metro
		inline bool lapsed() { return (millis() - mvarStartTime) > mvarTravelTime; } //Movimiento caducado
		void loop(); //Completa el bucle (para motores en marcha si llega el momento)
		void showConfig();
		void showStatus();

	private:
		void commonMotorInit(uint8_t motorEnablePort, uint8_t straightMotorPort, uint8_t curveMotorPort, uint16_t movingDelay);
		//Funciones para los motores.
		void startMotor(frogPosition rhs);
		void stopMotor();
		bool isMotorRunning();
		bool motorRunningDirection();

	};
}


#endif