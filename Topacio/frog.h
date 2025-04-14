//Esto es un modelo para un motor de aguja, que puede tener también comprobador de posición.
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
			straight, //Vía directa
			curve, //Vía desviada
			indeterminate //Ni vía directa, ni vía desviada
		};
	private:
		uint64_t mvarStartTime; //Tiempo en que comenzó a moverse el motor
		bool mvarUsesServo; //Flag que se establece al arrancar, que indica si vamos con servo o con controlador.
		bool mvarHasComprobation; //Flag que se establece al arrancar, que indica si esta aguja tiene comprobación.
		frogPosition mvarCurrentFrogPosition; //Valor que devuelve getPosition();
		void auxControllerStop(); //Para el movimiento del motor.

	protected:
		uint16_t mvarTravelTime; //Tiempo de movimiento en milisegundos (para obtener comprobación)
		Servo* mvarServo; //Motor controlador (si la aguja va por servo)
		uint8_t mcolPort[5]; //Puertos del motor.	
		//Configuración con servo:
		//mcolPort[0] --> puerto del servo.
		//mcolPort[1] --> ángulo de servo para vía directa.
		//mcolPort[2] --> ángulo de servo para vía desviada.
		//mcolPort[3] --> Fin de carrera directa (si hay)
		//mcolPort[4] --> Fin de carrera desviada (si hay)

		//Configuración con controlador:
		//mcolPort[0] --> Motor enabled 
		//mcolPort[1] --> Motor right
		//mcolPort[2] --> Motor left
		//mcolPort[3] --> Fin de carrera directa (si hay)
		//mcolPort[4] --> Fin de carrera desviada (si hay)
		frogPosition mvarTarget; //Destino del desvío

	public:
		FrogClass(); //Constructor
		void init(Servo* servo, uint8_t servoChannel, uint8_t straightAngle, uint8_t curveAngle); //Asigna los recursos a este desvío.
		void init(Servo* servo, uint8_t servoChannel, uint8_t straightAngle, uint8_t curveAngle, uint8_t straightStopPort, uint8_t curveStopPort); //Asigna los recursos a este desvío.
		void init(uint8_t motorEnablePort, uint8_t straightMotorPort, uint8_t curveMotorPort, uint16_t movingDelay); //Asigna los recursos a este desvío.
		void init(uint8_t motorEnablePort, uint8_t straightMotorPort, uint8_t curveMotorPort, uint8_t straightStopPort, uint8_t curveStopPort, uint16_t movingDelay); //Asigna los recursos a este desvío.
		void setPosition(frogPosition rhs); //Asigna la posición de este desvío.
		inline frogPosition getPosition() { return mvarCurrentFrogPosition; } //Obtiene la posición actual de este desvío.
		inline frogPosition getTarget() { return mvarTarget; } //Obtiene el objetivo del desvío.
		bool getCompleted(); //Indica si la posición coincide con la última orden cursada.
		inline void setCompleted() { mvarCurrentFrogPosition = getTarget(); } //Asigna la posición terminada.
		inline void setTravelTime(uint16_t rhs) { mvarTravelTime = rhs; }
		bool getComprobation(); //Indica si el desvío ha comprobado que la última posición es correcta.
		inline void timerStart() { mvarStartTime = millis(); } //Pone a contar el diferímetro
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