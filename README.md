# iSafe API

## Introduction
This is a REST API for a password manager android app **iSafe**

Here is a link to a postman collection with the API endpoints: [Postman collection](https://drive.google.com/file/d/1SwA90HSGaPAYAtFPrSSgw3JaY2MKbChJ/view?usp=sharing)

The API and the database are deployed on Azure

## Technologies
- **.Net** - framework for the web application
- **Asp.Net MVC** - for the web API
- **Dapper** - an ORM for interacting with the database
- **Microsoft SQL** - for data storage
- **Security, JWT** - for a secure authentication

## Architecture
- **MVC** - app architecture

## Features
### - Authentication
Allows to create and login to accounts

The endpoints POST Auth/Register and POST Auth/Login allow to create users and login to accounts:

![photo_2024-02-06_18-18-41](https://github.com/DanielJshn/iSafeAPI/assets/134506544/7e014eae-0024-406d-9cdf-58e5aae0381e)

### - Passwords
Allows to fetch the passwords list

The endpoint GET Password/GetPassword allows to get the passwords list of the logged in user:

![photo_2024-02-06_18-22-53](https://github.com/DanielJshn/iSafeAPI/assets/134506544/9349e3f3-ab80-45d5-b3ad-2e4b0cf2954b)

### - Add/Edit Password
Allows to add new and edit existing passwords

The endpoint POST Password/AddPassword creates a new password in the database,  PUT Password/UpdatePassword updates the password if it exists:

![photo_2024-02-06_18-26-51](https://github.com/DanielJshn/iSafeAPI/assets/134506544/3ec17194-a93f-43bd-bab7-ece037b21a18)

### - Delete a password
Allows to delete a specific password

The endpoint DELETE Password/DeletePassword removes the password from the database:

![photo_2024-02-06_18-29-13](https://github.com/DanielJshn/iSafeAPI/assets/134506544/261d0391-1f60-4352-816b-1d676af72d9c)

### - Delete all user data
Allows to delete a user and the user data from the database

The endpoint DELETE Auth/DeleteAllData removes all user related data from the database

### - Refresh token
Allows refresh the access token

The endpoint GET Auth/RefreshToken refreshes the user access token

## Get the mobile application

[Here](https://drive.google.com/file/d/1t1BguOmEW5mV7rWmeXgh_Z0xvYCVW41S/view?usp=sharing) is a link to the mobile application .apk. You can install it and see the API in action!## Architecture
