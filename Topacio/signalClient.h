//Objeto señal luminosa en un enclavamiento
#pragma once
#ifndef _SIGNAL_CLIENT_
#define _SIGNAL_CLIENT_
//Incluímos la referencia a la biblioteca de servos por si es un semáforo, aunque sea una señal luminosa que no tiene de eso.
#include <Servo.h>
#include "childClient.h"
#define SERVO_INCREMENT 2
#define ANGLE_VIA_LIBRE 90
#define ANGLE_AVISO_DE_PARADA 45
#define ANGLE_PARADA 0
namespace helper
{
    class signalClient : public childClient
    {
    public:
        static const char* const helper::signalClient::auxColores[];
        signalClient();
        inline helper::orderType getOrder() { return mvarOrder; }
        void setInverted(bool green, bool red, bool yellow, bool white); //Ajusta el mapa de inversiones.
        void setLedPort(uint8_t green, uint8_t red, uint8_t yellow, uint8_t white, uint8_t servo); //Ajusta los puertos de las luces.
        inline uint8_t getLedPort(uint8_t color) { return mcolLedPort[color]; }
        void init(uint8_t id); //Ajusta los sentidos de salida y prepara las señales para la primera visualización.
        virtual void setOrder(helper::orderType order); //Asigna la orden.
        void loop() override; //Hay que llamarla periódicamente
        void showConfig() override; //Muestra la configuración de un cliente concreto
        void showStatus() override; //Muestra el estado actual de este cliente

        //Código para semáforo
        void setServo(Servo* rhs, uint8_t servoPort, uint16_t stopAngle, uint16_t midAngle, uint16_t allowAngle, bool tomeuMode, bool dynamic, bool speed0, bool speed1);

        inline Servo* getServo() { return mvarServo; }
        inline void setSemaphoreSpeed(uint8_t speed) { mvarSemaphoreIncrement = speed; } //Puede ajustar la velocidad del semáforo de forma personalizada.

    protected:
        bool mcolInverted[5]; //Colección con las inversiones de los cuatro fuegos de una señal.
        //Atención: El bool 5 se usa para los semáforos "tipo Tomeu"
        uint8_t mcolLedPort[5]; //Colección con los cuatro fuegos de una señal.            
        //Atención: El puerto 5 se usa en los semáforos para la salida del servo.
        uint16_t mcolSemaphorePosition[3]; //Posición en ángulos del brazo para cada aspecto de la señal
        helper::orderType mvarOrder; //Última orden recibida
        virtual void auxSetOrder(); //Asigna la última orden recibida        
    private:
        //Código común
        uint8_t mvarLights; //Flags con las luces que se deben encender.
        void auxRefresh(); //Escribe la constante de las luces en las salidas del Arduino.
        //Código para semáforo        
        Servo* mvarServo; //Referencia al servo (si es que hay)
        uint8_t mvarServoPort; //Número de puerto del servo
        uint16_t mvarSemaphorePosition; //Posición actual que tiene el brazo del semáforo.
        uint16_t mvarSemaphoreTarget; //Posición a la que debería tender el brazo del semáforo.
        uint8_t mvarSemaphoreIncrement; //Factor de velocidad del semáforo.
        bool mvarTomeuMode; //Modo Tomeu.
        bool mvarDynamic; //Gestión dinámica
        uint8_t mvarSpeedDelay; //Retardo del servo en movimiento
    };
}
#endif
