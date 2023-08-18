# Plane-Ticket-Booking-System

## Project Overview 
------------------------------------------------------------------
This D# programming project implements a simplified plane ticket booking system involving airlines and travel agencies. The goal of this project is to exercise various concepts learned in the chapter. It simulates the interaction between airlines and travel agencies in a multithreaded environment. The project includes components such as airlines, travel agencies, pricing models, order processing, encoding, decoding, and a multi-cell buffer for communication.

## Architecture
The project architecture consists of multiple components that interact to simulate the plane ticket booking system:

### 1. Airlines (Airline1, Airline2, ...):

- Represents airlines that supply blocks of seats to travel agencies.
- Each airline uses a pricing model to calculate ticket prices.
- Emits a price-cut event if a new price is lower than the previous price.
- Receives orders from the multi-cell buffer, decodes them, and processes orders.

### 2. PricingModel:

- A class/method in the Airline class that determines seat prices based on a mathematical model.
- Prices can increase or decrease based on factors like order volume and availability.

### 3. OrderProcessing:

- A class/method in the Airline that processes orders.
- Validates credit card numbers and calculates the total amount.
- Processes orders in separate threads.

### 4. TravelAgencies (TravelAgency1, TravelAgency2, ...):

- Represents travel agencies that book blocks of seats from airlines.
- Subscribes to price-cut events emitted by airlines.
- Evaluates prices, generates OrderObjects, encodes them, and sends them to the multi-cell buffer.

### 5. OrderClass:

- Represents an order with senderID, cardNo, receiverID, and amount.
- Used to create instances of OrderObjects.

### 6. MultiCellBuffer:

- Used for communication between travel agencies and airlines.
- Contains multiple data cells (e.g., n=3) for storing encoded orders.
- Airlines read orders intended for them.

### 7. Encoder:

- Converts an OrderObject into an encoded string.
- Encoded string is sent to the multi-cell buffer.

### 8. Decoder:

- Converts an encoded string back into an OrderObject.

### 9. Main:

- Main thread initializes the system, creates objects, and starts threads.

