/*
 Name:		Topacio.ino
 Created:	11/08/2024 14:56:29
 Modified:  13/04/2025 17:00:00
 Author:	ErPe
*/

// the setup function runs once when you press reset or power the board
#pragma once
#include <SoftwareSerial.h>
#include "MontefaroSignal.h"
#include <MultiFuncShield.h>
#include "MtfClient.h"


helper::MtfClient mvarClient = helper::MtfClient();

void setup() {
	mvarClient.init();

}

// the loop function runs over and over again until power down or reset
void loop() {
	mvarClient.loop();
}