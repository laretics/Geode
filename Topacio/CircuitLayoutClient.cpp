#include "circuitLayoutClient.h"
helper::circuitLayoutClient::circuitLayoutClient()
{
	mvarLayoutsCount = 0;
	mvarFrogsCount = 0;
}
void helper::circuitLayoutClient::init(uint8_t id)
{
	childClient::init(id);
	//Por defecto, para itinerarios no especificados, las agujas se colocan a más.
	for (uint8_t la = 0; la < MAX_LAYOUTS; la++)
	{
		for (uint8_t fr = 0; fr < MAX_FROGS_PER_LAYOUT; fr++)
		{
			mcolLayouts[la][fr] = true;
		}
	}
	for (uint8_t i = 0; i < mvarFrogsCount; i++)
	{
		mcolFrog[i]->setTravelTime(mvarFrogMovingDelay);
	}
}
void helper::circuitLayoutClient::addFrog(FrogClass* rhs)
{
	if (mvarFrogsCount < MAX_FROGS_PER_LAYOUT)
	{
		mcolFrog[mvarFrogsCount++] = rhs;
	}
}
void helper::circuitLayoutClient::setLayoutFrog(uint8_t layoutIndex, uint8_t frogId, bool straight) //Asigna un estado de aguja según la configuración
{
	if (layoutIndex + 1 > mvarLayoutsCount)
		mvarLayoutsCount = layoutIndex + 1; //Ampliamos el límite de configuraciones.
	if (frogId < mvarFrogsCount)
		mcolLayouts[layoutIndex][frogId] = straight; //Asignación en el array.
}
void helper::circuitLayoutClient::setLayout(uint8_t layoutIndex) //Ordena a los aparatos de vía disponerse para cumplir con el itinerario asignado.
{
	mvarFrogMovingIndex = 0;
	mvarTargetLayout = layoutIndex;
	mvarTimeStamp = millis(); //Pongo un cronómetro para mirar si hay error de comprobación.
}
uint8_t helper::circuitLayoutClient::getLayout()
{
	return mvarTargetLayout;
}
bool helper::circuitLayoutClient::layoutSetCompleted() { return mvarFrogMovingIndex >= mvarFrogsCount; }
void helper::circuitLayoutClient::loop()
{
	//Primero comprobamos que haya terminado de moverse el enclavamiento
	if (layoutSetCompleted()) return;
	if (mvarSimultaneous)
	{
		//Luego nos ponemos con el movimiento simultáneo... ahora no toca.
	}
	else
	{
		pp(F("Aguja ")); pp(mvarFrogMovingIndex); pp(F(": "));
		helper::FrogClass* auxFrog = mcolFrog[mvarFrogMovingIndex];
		bool currentTargetStraight = mcolLayouts[mvarTargetLayout][mvarFrogMovingIndex];
		//Compruebo si la aguja del índice ha sido asignada con la posición que busco.
		if (auxFrog->getTarget() == getCurrentFrogTarget(mvarFrogMovingIndex))
		{
			if (!auxFrog->getCompleted())
			{
				//El desvío ya está en marcha. Hay que comprobar cuándo llega a su destino.
				auxFrog->loop();
				pp(F("Loop "));
			}
			else
			{
				pp(F("Terminó!"));
				mvarFrogMovingIndex++; //Este desvío ha terminado... pasamos al siguiente.
			}
		}
		else
		{
			//El desvío todavía no ha recibido la orden de movimiento.
			auxFrog->setPosition(getCurrentFrogTarget(mvarFrogMovingIndex));
			pp(F("Asignando posición"));
		}
		pl();
	}
}
bool helper::circuitLayoutClient::hasError()
{
	if (0 == mvarFrogMovingDelay || layoutSetCompleted()) return false;
	return(millis() > (uint64_t(mvarFrogMovingDelay) * 1000) + mvarTimeStamp);
}
void helper::circuitLayoutClient::showConfig()
{
	pp(F("LayoutClient id "));
	childClient::showConfig(); pl();
	pp(F("Frogs: ")); pp(mvarFrogsCount); pl();
	for (uint8_t i = 0; i < mvarFrogsCount; i++)
	{
		pp(F("Frog ")); pp(i); pl();
		mcolFrog[i]->showConfig();
	}
	pl();
	pp(F("Itineraries ")); pp(mvarLayoutsCount); pl();
	pp(F("Layout"));
	for (uint8_t i = 0; i < mvarFrogsCount; i++)
	{
		pt(); pp(F("F")); pp(i);
	}
	pl();
	for (uint8_t j = 0; j < mvarLayoutsCount; j++)
	{
		pp(j);
		for (uint8_t i = 0; i < mvarFrogsCount; i++)
		{
			pt();
			if (mcolLayouts[j][i])
				pp(F("dir"));
			else
				pp(F("cur"));
		}
		pl();
	}
	pp(F("Moving delay: ")); pp(mvarFrogMovingDelay);
	if (mvarSimultaneous)
		pp(F(" simultaneous."));
	else
		pp(F(" step by step."));
}
void helper::circuitLayoutClient::showStatus()
{
	pp(F("LayoutClient id "));
	childClient::showStatus(); pl();
	pp(F("Current target layout: ")); mvarTargetLayout;
	if (layoutSetCompleted())
		pp(F(". Iddle."));
	else
	{
		if (hasError())
			pp(F(" not reached: ERROR"));
		else
			pp(F("... in progress"));
	}
	pl();
	pp(F("Target configuration: ")); pl();
	for (uint8_t i = 0; i < mvarFrogsCount; i++)
	{
		pp(F("F")); pp(i); pt();
	}
	for (uint8_t i = 0; i < mvarFrogsCount; i++)
	{
		if (mcolLayouts[mvarTargetLayout][i])
			pp(F("dir"));
		else
			pp(F("cur"));
		pt();
	}
	for (uint8_t i = 0; i < mvarFrogsCount; i++)
	{
		pp(F("Frog ")); pp(i); pp(F(":")); pl();
		mcolFrog[i]->showStatus();
	}
}
helper::FrogClass::frogPosition helper::circuitLayoutClient::getCurrentFrogTarget(uint8_t frogId)
{
	if (mcolLayouts[mvarTargetLayout][frogId])
		return helper::FrogClass::frogPosition::straight;
	else
		return helper::FrogClass::frogPosition::curve;
}