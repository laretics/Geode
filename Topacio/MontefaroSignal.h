/*
 Name:		MontefaroSignal.h
 Created:	09/08/2024 9:32:32
 Author:	ErPe
 Editor:	http://www.visualmicro.com
*/

#pragma once
#ifndef _MontefaroSignal_h
#define _MontefaroSignal_h
#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include <Ethernet.h>
#include <EEPROM.h>
#include <Servo.h>

#include "helper.h"
#include "childClient.h"
#include <Client.h>
#include "signalClient.h"
#include "Client.h"
#include "frog.h"
#include "MontefaroSignal.h"
#include "circuitLayoutClient.h"
#include "circuitDetectorClient.h"



#endif