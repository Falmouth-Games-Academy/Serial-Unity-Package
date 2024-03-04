#include <Arduino.h>

char handshake = 'x';
bool ledState = false;

void setup() {
  
  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, LOW);

  Serial.begin(9600);
  bool online = false;
  while(!online){
    while(!Serial.available()){
    }
    char inp = Serial.read();
    if(inp == handshake){
      online = true;
      Serial.println(handshake);
    }
  }
}


void loop() {

  if(Serial.available()){
    char inp = Serial.read();
    if(inp == 'a'){
      Serial.println(analogRead(A0));
    }
    if(inp == 'b'){
      ledState = !ledState;
      digitalWrite(LED_BUILTIN, ledState);
    }
  }

}
