#include "signalClient.h"

const char* const helper::signalClient::auxColores[] = { "Green","Red","Yellow","White" };
helper::signalClient::signalClient() :childClient()
{
    mvarOrder = helper::orderType::toParada;
    channelId = NULL_CHANNEL; //Dirección nula.
    for (uint8_t i = 0; i < 5; i++)
        mcolLedPort[i] = INVALID_PORT; //Inicio los puertos en una dirección nula.
    mvarSemaphoreIncrement = SERVO_INCREMENT;
    mvarServo = NULL;
    //Valores por defecto para la orientación del servo.
    mcolSemaphorePosition[0] = ANGLE_PARADA;
    mcolSemaphorePosition[1] = ANGLE_AVISO_DE_PARADA;
    mcolSemaphorePosition[2] = ANGLE_VIA_LIBRE;
}
void helper::signalClient::setInverted(bool green, bool red, bool yellow, bool white)
{
    mcolInverted[0] = green;
    mcolInverted[1] = red;
    mcolInverted[2] = yellow;
    mcolInverted[3] = white;
}
void helper::signalClient::setLedPort(uint8_t green, uint8_t red, uint8_t yellow, uint8_t white, uint8_t servo) //Ajusta los puertos de las luces.
{
    mcolLedPort[0] = green;
    mcolLedPort[1] = red;
    mcolLedPort[2] = yellow;
    mcolLedPort[3] = white;
    mcolLedPort[4] = servo;
}
void helper::signalClient::setServo(Servo* rhs, uint8_t servoPort, uint16_t stopAngle, uint16_t midAngle, uint16_t allowAngle, bool tomeuMode, bool dynamic, bool speed0, bool speed1)
{
    mvarServo = rhs;
    mvarServoPort = servoPort;
    mcolSemaphorePosition[0] = stopAngle;
    mcolSemaphorePosition[1] = midAngle;
    mcolSemaphorePosition[2] = allowAngle;
    mvarTomeuMode = tomeuMode;
    mvarDynamic = dynamic;
    mvarSpeedDelay = 0;
    if (!speed0) mvarSpeedDelay = bitSet(mvarSpeedDelay, 3);
    if (!speed1) mvarSpeedDelay = bitSet(mvarSpeedDelay, 4);
}
void helper::signalClient::init(uint8_t id)
{
    childClient::init(id);
    uint8_t auxPort;
    for (uint8_t i = 0; i < 4; i++)
    {
        auxPort = getLedPort(i);
        if (INVALID_PORT != auxPort) pinMode(auxPort, OUTPUT);
    }
    mvarSemaphorePosition = mcolSemaphorePosition[0];
    mvarSemaphoreTarget = mcolSemaphorePosition[0];
    auxSetOrder();
}
void helper::signalClient::setOrder(helper::orderType order)
{
    mvarOrder = order;
    auxSetOrder();
}

void helper::signalClient::loop()
{
    //Loop sólo tiene sentido con el servo.
    if (NULL == mvarServo) return;
    if (mvarSemaphorePosition != mvarSemaphoreTarget)
    {
        if (mvarSemaphorePosition > mvarSemaphoreTarget)
        {
            if (mvarSemaphorePosition - mvarSemaphoreTarget > mvarSemaphoreIncrement)
            {
                mvarSemaphorePosition -= mvarSemaphoreIncrement;
                //pp(F("--Servo pos: ")); pp(mvarSemaphorePosition); pl();
            }
            else
            {
                mvarSemaphorePosition = mvarSemaphoreTarget; //Fin de recorrido
                //pp(F("Servo ended trip")); pl();
            }
        }
        else
        {
            if (mvarSemaphoreTarget - mvarSemaphorePosition > mvarSemaphoreIncrement)
            {
                mvarSemaphorePosition += mvarSemaphoreIncrement;
                //pp(F("++Servo pos: ")); pp(mvarSemaphorePosition); pl();
            }
            else
            {
                mvarSemaphorePosition = mvarSemaphoreTarget; //Fin de recorrido
                //pp(F("Servo ended trip")); pl();
            }
        }
        auxSetOrder();
    }

    //Muevo el servo a la posición    
    if (mvarSemaphorePosition == mvarSemaphoreTarget)
    {//Fin de recorrido
        if (mvarDynamic)
        {
            //Desconectamos el motor.
            if (mvarServo->attached())
            {
                mvarServo->detach();
                //pp(F("Servo dettached")); pl();
            }

        }
    }
    else
    {//A mitad del recorrido
        if (!(mvarServo->attached()))
        {
            mvarServo->attach(mvarServoPort);
            //pp(F("Servo attached")); pl();
        }
        //pp(F("Servo ")); pp(mvarSemaphorePosition); pl();
        delay(mvarSpeedDelay);
        mvarServo->write(mvarSemaphorePosition);
    }
}
void helper::signalClient::showConfig()
{
    pp(F("Signal "));
    childClient::showConfig(); pl();
    pp(F("Color\tPort\tInverted")); pl();
    for (uint8_t i = 0; i < 4; i++)
    {
        if (NULL_CHANNEL != mcolLedPort[i])
        {
            pp(auxColores[i]);
            pt();
            pp(mcolLedPort[i]);
            pt();
            if (mcolInverted[i])
                pp(F("Yes"));
            pl();
        }
    }
    if (NULL != mvarServo)
    {
        pp(F("Servo port: A")); pp(mcolLedPort[4] - A0);  pl();
        pp(F("Open angle: ")); pp(mcolSemaphorePosition[2]); pl();
        pp(F("Mid angle: ")); pp(mcolSemaphorePosition[1]); pl();
        pp(F("Stop angle: ")); pp(mcolSemaphorePosition[0]); pl();
        pp(F("Arm speed: ")); pp(mvarSemaphoreIncrement); pl();
        if (INVALID_PORT != mcolLedPort[0] || INVALID_PORT != mcolLedPort[1])
        {
            if (mvarTomeuMode)
                pp(F("Tomeu (lights off) semaphore"));
            else
                pp(F("Normal light semaphore"));
            pl();
            pp(F("Speed delay: ")); pp(mvarSpeedDelay); pl();
            if (mvarDynamic)
            {
                pp(F("Dynamic mode on"));
                pl();
            }
        }
    }
}
void helper::signalClient::showStatus()
{
    pp(F("Signal "));
    childClient::showStatus(); pl();
    pp(F("Order: ")); helper::orderString(mvarOrder); pl();
    pp(F("Color\tLight\tOutput")); pl();
    for (uint8_t i = 0; i < 4; i++)
    {
        if (NULL_CHANNEL != mcolLedPort[i])
        {
            pp(auxColores[i]);
            pt();
            if (bit_check(mvarLights, i))
                pp(F("###"));
            pt();
            if (bit_check(mvarLights, i) ^ mcolInverted[i])
                pp(F("-->"));
            pl();
        }
    }
    if (NULL != mvarServo)
    {
        pp(F("Arm target: ")); pp(mvarSemaphoreTarget); pl();
        pp(F("Arm position: ")); pp(mvarSemaphorePosition); pl();
    }
}
void helper::signalClient::auxSetOrder()
{
    //BitArray mvarLights:
    //0000gryb
    switch (mvarOrder)
    {
    case helper::orderType::toViaLibre:
    {
        mvarSemaphoreTarget = mcolSemaphorePosition[2];
        mvarLights = 0b00000001;
        break;
    }
    case helper::orderType::toParada:
    {
        mvarSemaphoreTarget = mcolSemaphorePosition[0];
        mvarLights = 0b00000010;
        break;
    }
    case helper::orderType::toAvisoDeParada:
    {
        mvarSemaphoreTarget = mcolSemaphorePosition[1];
        mvarLights = 0b00000100;
        break;
    }
    case helper::orderType::toPrecaucion:
    {
        mvarSemaphoreTarget = mcolSemaphorePosition[1];
        mvarLights = 0b00000101;
        break;
    }
    case helper::orderType::toRebaseAutorizado:
    {
        mvarSemaphoreTarget = mcolSemaphorePosition[0];
        mvarLights = 0b00001010;
        break;
    }
    default:
    {
        mvarSemaphoreTarget = mcolSemaphorePosition[0];
        mvarLights = 0;
        break;
    }
    }
    //Modo Tomeu: Apagamos las luces del semáforo hasta que llegue a su destino.
    if ((mvarSemaphorePosition != mvarSemaphoreTarget) && mvarTomeuMode)
        mvarLights = 0;

    auxRefresh();
}
void helper::signalClient::auxRefresh()
{
    bool encendido;
    for (uint8_t i = 0; i < 4; i++)
    {
        if (INVALID_PORT != mcolLedPort[i])
        {
            //pp(F("Encendido = bit_check(")); pp(mvarLights); pp(F(",")); pp(i); pp(F(");")); pl();
            encendido = bit_check(mvarLights, i);
            digitalWrite(mcolLedPort[i], mcolInverted[i] ^ encendido);
        }
    }

}
