// 
// 
// 

#include "helper.h"

void helper::pp(IPAddress rhs)
{
	for (int i = 0; i < 4; i++)
	{
		if (i > 0)
		{
			pp(F("."));
		}
		pp(rhs[i]);
	}
}
   void helper::pp(uint8_t* mac)
   {
	   for (int i = 0; i < 6; i++)
	   {
		   if (mac[i] < 16) { pp(F("0")); }
		   Serial.print(mac[i], HEX);
		   if (i < 5) { pp(F(":")); }
	   }
   }
   void helper::pl(uint8_t num)
   {
	   for (uint8_t i = 0; i < num; i++) 
		   COM_PORT_FOR_DEBUG.println();
   } 
   void helper::ps(uint8_t size)
   {
	   for (uint8_t i = 0; i < size; i++)
		   COM_PORT_FOR_DEBUG.print(F("="));
	   pl();
   }
   void helper::pt(uint8_t num)
   {
	   for (uint8_t i = 0; i < num; i++) 
		   COM_PORT_FOR_DEBUG.print(F("\t"));
   }
   void helper::timeStamp()
   {
	   pp(F("Tme:")); COM_PORT_FOR_DEBUG.print(millis()); pl();
   }
