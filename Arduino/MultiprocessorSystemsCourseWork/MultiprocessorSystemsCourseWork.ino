///// HCSR4 ///// DistanceSensor
#include <HCSR04.h>
#define PIN_TRIG 12
#define PIN_ECHO 11
#define TRIGGER_DISTANCE_IN_CM 5 // Дистанция, которая считается дистанцией обнаружения движения
int distanceInCm;
HCSR04 hc(PIN_TRIG, PIN_ECHO);

///// TMB12 ///// Buzzer
#define PIN_BUZZER 10

///// MQ-2 ///// SmokeAnalizer
#define PIN_MQ2 A0
float MQ2SensorValue;  // переменная для хранения значения датчика

///// DHT11 ///// TemperatureAndHumiditySensor
#include "DHT.h"
#define PIN_DHT 9
DHT dht(PIN_DHT, DHT11);
float humidity;
float temperature;

bool isSomethingDetected = false; // Определяет, было ли обнаружено что-то

void setup() {
  // Инициализируем взаимодействие по последовательному порту
  Serial.begin (9600);
  InitializeBuzzer();
  InitializeMQ2();
  InitializeDHT11();
  delay(2000); // Время необходимое для инициализации датчиков (конкретно MQ2)
}

void InitializeBuzzer()
{
  pinMode(PIN_BUZZER, OUTPUT);
  digitalWrite(PIN_BUZZER, HIGH);
}

void InitializeMQ2()
{
  Serial.println("MQ2 is warming up!");
  Serial.println("----- MQ2 initialized -----");
}

void InitializeDHT11()
{
  dht.begin();
}

void loop() {
  ScanHCSR4();
  ScanDHT11();
  ScanMQ2();

  if (isSomethingDetected)
    digitalWrite(PIN_BUZZER, LOW);
  else
    digitalWrite(PIN_BUZZER, HIGH);

  delay(500);
}

void ScanDHT11() // DHT11
{
  humidity = dht.readHumidity(); //Измеряем влажность
  temperature = dht.readTemperature(); //Измеряем температуру

  if (isnan(humidity) || isnan(temperature)) {  // Проверка. Если не удается считать показания, выводится «Ошибка считывания», и программа завершает работу
    Serial.println("DHT11: Scan error");
  }
  Serial.print("Humidity: ");
  Serial.print(humidity);
  Serial.print(" %\t");
  Serial.print("Temperature: ");
  Serial.print(temperature);
  Serial.println(" *C "); //Вывод показателей на экран
}

void ScanMQ2()
{
  MQ2SensorValue = analogRead(PIN_MQ2);
  Serial.print("MQ2 value: ");
  Serial.println(MQ2SensorValue);

  Serial.println("");
//  delay(2000); // подождать 2 сек до следующего чтения
}

void ScanHCSR4() // HCSR4
{
  // Стартовая задержка, необходимая для корректной работы.
  delay(50);

  // Получаем значение от датчика расстояния и сохраняем его в переменную
  distanceInCm = hc.dist();

  // Печатаем расстояние в мониторе порта
  Serial.print("Distance: ");
  Serial.print(distanceInCm);
  Serial.println(" cm");

  if (distanceInCm < TRIGGER_DISTANCE_IN_CM && distanceInCm != 0)
    isSomethingDetected = true;
  else
    isSomethingDetected = false;
}
