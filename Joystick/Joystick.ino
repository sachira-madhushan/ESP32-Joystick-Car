int xpin=12;
int ypin=13;
int centerX=1951;
int centerY=1901;
void setup() {
  Serial.begin(115200);
  Serial.print("Connected");
}

void loop() {
  int xvalue=analogRead(xpin);
  int yvalue=analogRead(ypin);

  int mappedX = map(xvalue, centerX - 100, centerX + 100, -1, 1);
  int mappedY = map(yvalue, centerY - 100, centerY + 100, -1, 1);

  Serial.print(mappedX);
  Serial.print(",");
  Serial.print(mappedY);
  Serial.println();
  delay(1000);
}
