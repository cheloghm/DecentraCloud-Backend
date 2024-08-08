# DecentraCloud Central Server

## Overview

DecentraCloud Central Server is the backend service for managing decentralized file storage. This server provides functionalities such as user authentication, file upload/download, file sharing, and node management. The files are stored on different storage nodes, ensuring security and redundancy.

## Features

- **User Authentication**: Register, login, and manage user details.
- **File Management**: Upload, view, download, and delete files.
- **File Sharing**: Share files with other users and revoke access.
- **Node Management**: Register and manage storage nodes.
- **Secure File Storage**: Files are encrypted and stored with UUIDs for added security.

## Prerequisites

- [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0)
- [MongoDB](https://www.mongodb.com/try/download/community)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)

## Setup

1. **Clone the Repository**

    ```bash
    git clone https://github.com/cheloghm/DecentraCloud.git
    cd DecentraCloud/Backend/DecentraCloud
    ```

2. **Configure MongoDB**

    Ensure MongoDB is installed and running on your machine or configure the connection string to point to your MongoDB instance.

3. **Configure the Application**

    Update the `appsettings.json` file with your MongoDB connection string and JWT settings.

    ```json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "ConnectionStrings": {
        "MongoDb": "mongodb://localhost:27017",
        "DatabaseName": "DecentraCloud"
      },
      "Jwt": {
        "Key": "ThisIsASecretKeyWithAtLeast32Cha",
        "Issuer": "DecentraCloudAPI",
        "Audience": "DecentraCloudClient"
      }
    }
    ```

4. **Run the Application**

    Open the project in Visual Studio 2022 and run the project. Alternatively, you can use the command line:

    ```bash
    dotnet run --project DecentraCloud.API
    ```

5. **Access the API Documentation**

    Once the application is running, navigate to `https://localhost:7240/swagger` to access the Swagger API documentation.

## Endpoints

### User Authentication

- **Register**

POST /api/Auth/register
Body: UserRegistrationDto { Username, Email, Password }

markdown
Copy code

- **Login**
POST /api/Auth/login
Body: UserLoginDto { Email, Password }

markdown
Copy code

- **Get User Details**
GET /api/Auth/me
Headers: Authorization: Bearer <token>

markdown
Copy code

### File Management

- **Upload File**
POST /api/File/upload
Headers: Authorization: Bearer <token>
Form Data: file (IFormFile)

markdown
Copy code

- **View File**
GET /api/File/view/{fileId}
Headers: Authorization: Bearer <token>

markdown
Copy code

- **Download File**
GET /api/File/download/{fileId}
Headers: Authorization: Bearer <token>

markdown
Copy code

- **Delete File**
DELETE /api/File/{fileId}
Headers: Authorization: Bearer <token>

markdown
Copy code

- **Search Files**
GET /api/File/search?query={filename}
Headers: Authorization: Bearer <token>

markdown
Copy code

### File Sharing

- **Share File**
POST /api/File/share/{fileId}
Headers: Authorization: Bearer <token>
Body: ShareFileDto { UserEmail }

markdown
Copy code

- **Revoke Share**
POST /api/File/revoke/{fileId}
Headers: Authorization: Bearer <token>
Body: RevokeShareDto { UserEmail }

markdown
Copy code

### Node Management

- **Register Node**
POST /api/Nodes/register
Body: NodeRegistrationDto { Email, Password, Storage, NodeName }

markdown
Copy code

- **Login Node**
POST /api/Nodes/login
Body: NodeLoginDto { NodeName, Email, Password, Endpoint }

markdown
Copy code

- **Update Node Status**
POST /api/Nodes/status
Headers: Authorization: Bearer <token>
Body: NodeStatusDto { NodeId, Uptime, Downtime, StorageStats, IsOnline, CauseOfDowntime }

markdown
Copy code

- **Get All Nodes**
GET /api/Nodes/all
Headers: Authorization: Bearer <token>

markdown
Copy code

### Node Management for User

- **Get User Nodes**
GET /api/NodeManagement/nodes
Headers: Authorization: Bearer <token>

markdown
Copy code

- **Update Node**
PUT /api/NodeManagement/node
Headers: Authorization: Bearer <token>
Body: Node { Id, NodeName, Storage, ... }

markdown
Copy code

- **Delete Node**
DELETE /api/NodeManagement/node/{nodeId}
Headers: Authorization: Bearer <token>

markdown
Copy code

## Contributing

1. **We will post out information as soon as we have gotten the project to a ripe stage to open it for contributions**

## License

This project is licensed under the MIT License.
