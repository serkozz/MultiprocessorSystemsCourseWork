///// HCSR4 ///// DistanceSensor
#include <HCSR04.h>
#define PIN_TRIG 6
#define PIN_ECHO 4
#define TRIGGER_DISTANCE_IN_CM 5 // Дистанция, которая считается дистанцией обнаружения движения
int distanceInCm;
HCSR04 hc(PIN_TRIG, PIN_ECHO);

///// TMB12 ///// Buzzer
#define PIN_BUZZER 2

///// MQ-2 ///// SmokeAnalizer
#define PIN_MQ2 A0
float MQ2SensorValue;  // переменная для хранения значения датчика

///// DHT11 ///// TemperatureAndHumiditySensor
#include "DHT.h"
#define PIN_DHT 3
DHT dht(PIN_DHT, DHT11);
float humidity;
float temperature;

///// RC522 RFIDSensor /////
#include <SPI.h>
#include <MFRC522.h>

#define RST_PIN 9
#define SS_PIN 10

MFRC522 mfrc522(SS_PIN, RST_PIN);  // Create MFRC522 instance

bool state = false;
bool isSomethingDetected = false; // Определяет, было ли обнаружено что-то

void setup() {
  // Инициализируем взаимодействие по последовательному порту
  Serial.begin (9600);
  InitializeBuzzer();
  InitializeMQ2();
  InitializeDHT11();
  InitializeRFID();
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
  delay(2000); // подождать 2 сек до разогрева датчика дыма
  Serial.println("----- MQ2 initialized -----");
}

void InitializeDHT11()
{
  dht.begin();
}

void InitializeRFID() {
  while (!Serial);    // Do nothing if no serial port is opened (added for Arduinos based on ATMEGA32U4)
  SPI.begin();      // Init SPI bus
  mfrc522.PCD_Init();   // Init MFRC522
  delay(4);       // Optional delay. Some board do need more time after init to be ready, see Readme
    mfrc522.PCD_DumpVersionToSerial();  // Show details of PCD - MFRC522 Card Reader details
    Serial.println(F("Scan PICC to see UID, SAK, type, and data blocks..."));
}

bool GetSystemState()
{
  // Reset the loop if no new card present on the sensor/reader. This saves the entire process when idle.
  if ( ! mfrc522.PICC_IsNewCardPresent()) {
    return;
  }

  // Select one of the cards
  if ( ! mfrc522.PICC_ReadCardSerial()) {
    return;
  }

  /// CardCode is: E0 5B D9 1B
  if (mfrc522.uid.uidByte[0] == 0xE0 &&
      mfrc522.uid.uidByte[1] == 0x5B &&
      mfrc522.uid.uidByte[2] == 0xD9 &&
      mfrc522.uid.uidByte[3] == 0x1B) {
    state = !state;
    delay(3000);
    return state;
  }
  else
    return state;
}

void loop() {
  while (GetSystemState() == true)
  {
    ScanHCSR4();
    ScanDHT11();
    ScanMQ2();

    if (isSomethingDetected)
      digitalWrite(PIN_BUZZER, LOW);
    else
      digitalWrite(PIN_BUZZER, HIGH);

    delay(500);
  }
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
