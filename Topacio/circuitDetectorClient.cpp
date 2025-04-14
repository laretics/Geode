#include "circuitDetectorClient.h"
helper::circuitDetectorClient::circuitDetectorClient()
{
	mvarOccupied = false;
	mvarOccupancyDetectorPort = INVALID_PORT;
	mvarFreeOccupancyDelay = 0; //Por defecto no tenemos retardo de liberación.
}
void helper::circuitDetectorClient::init(uint8_t id)
{
	childClient::init(id);
	if (INVALID_PORT != mvarOccupancyDetectorPort)
		pinMode(mvarOccupancyDetectorPort, OUTPUT);
}
void helper::circuitDetectorClient::init(uint8_t id, uint8_t port)
{
	mvarOccupancyDetectorPort = port;
	circuitDetectorClient::init(id);
}
void helper::circuitDetectorClient::setOccupancy(bool occupancy)
{
	if (occupancy) //Ocupación
	{
		mvarTimeStamp = millis(); //Ponemos en marcha el contador (incluso aunque no tengamos liberación automática)
	}
	mvarOccupied = occupancy;
}
uint64_t helper::circuitDetectorClient::getOccupancyLapse()
{
	if (!mvarOccupied) return 0;
	return (millis() - mvarTimeStamp) / 1000;
}
void helper::circuitDetectorClient::loop()
{
	if (0 != mvarFreeOccupancyDelay && getOccupancy())
	{
		if (millis() > (uint64_t(mvarFreeOccupancyDelay) * 1000) + mvarTimeStamp)
			setOccupancy(false); //Ha pasado el tiempo de liberación automática.
	}
}
void helper::circuitDetectorClient::showConfig()
{
	pp(F("Circuit detector "));
	childClient::showConfig(); pl();
	pp(F("Input port: ")); pp(mvarOccupancyDetectorPort); pl();
	if (0 == mvarFreeOccupancyDelay)
	{
		pp(F("Manual occupancy release."));
	}
	else
	{
		pp(F("Auto release after ")); pp(mvarFreeOccupancyDelay); pp(F(" seconds."));
	}
	pl();
}
void helper::circuitDetectorClient::showStatus()
{
	pp(F("Circuit detector "));
	childClient::showStatus(); pl();
	if (mvarOccupied)
	{
		uint64_t lapso = getOccupancyLapse();
		pp(F("Occupied ")); pp(lapso); pp(F(" seconds ago."));
		if (0 != mvarFreeOccupancyDelay)
		{
			pl(); pp(F("Will be free in ")); pp(mvarFreeOccupancyDelay - lapso); pp(F(" seconds."));
		}
	}
	else
	{
		pp(F("Free"));
	}
	pl();
}