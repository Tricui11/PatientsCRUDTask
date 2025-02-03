# Patient API

This is a RESTful API for managing patient records, developed using .NET 6 and running in Docker containers.

## Features

- CRUD operations for `Patient` entity.
- Search by `birthDate` following [FHIR date search](https://www.hl7.org/fhir/search.html#date) specifications.
- API documentation with Swagger.
- A console application to generate and add 100 patients via the API.
- Dockerized setup for easy deployment.
- Postman collection for API testing.

## Entity Model (JSON Example)

```json
{
  "name": {
    "id": "d8ff176f-bd0a-4b8e-b329-871952e32e1f",
    "use": "official",
    "family": "Иванов",
    "given": [
      "Иван",
      "Иванович"
    ]
  },
  "gender": "male",
  "birthDate": "2024-01-13T18:25:43",
  "active": true
}
```

### Required Fields:
- `name.family`
- `birthDate`

### Enumerations:
- `gender`: `male` | `female` | `other` | `unknown`
- `active`: `true` | `false`

## Running the Application with Docker

1. Ensure you have [Docker](https://www.docker.com/) installed.
2. Clone the repository:
   ```sh
   git clone https://github.com/Tricui11/PatientsCRUDTask.git
   cd patient-api
   ```
3. Build and run the application:
   ```sh
   docker-compose up --build
   ```
4. The API will be available at:  
   ```
   http://localhost:5000
   ```
5. Open Swagger UI to explore the API:  
   ```
   http://localhost:5000/swagger/index.html
   ```

## Console Application for Data Generation

A console application (`PatientsGenerationApp`) is included to populate the database with sample data. It generates and adds 100 patient records to the API.

To run the console application inside the Docker container:
```sh
docker-compose run patientsgenerationapp
```

Alternatively, to run it locally:
```sh
dotnet run --project PatientsGenerationApp
```

## API Testing with Postman

A Postman collection is included in the repository for testing API endpoints.  
**File location:**  
```
Patient API Tests.postman_collection.json
```
Import this file into Postman and use the predefined requests to test:

- Create a patient
- Update a patient
- Retrieve a patient by ID
- Delete a patient
- Search patients by `birthDate`
