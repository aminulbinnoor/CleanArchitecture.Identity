### **Authentication Endpoints:**

1. **POST** `/api/auth/register` - Register new user
2. **POST** `/api/auth/login` - Login and get tokens
3. **POST** `/api/auth/logout` - Logout and revoke token
4. **POST** `/api/auth/refresh-token` - Refresh access token

### **User Endpoints (Require Authentication):**

5. **GET** `/api/users/me` - Get current user info
6. **GET** `/api/users` - Get all users (Admin only)
7. **GET** `/api/users/{id}` - Get user by ID (Admin only)
8. **PUT** `/api/users/{id}/roles` - Update user roles (Admin only)

### **Test Endpoints:**

9. **GET** `/api/auth/test` - Test authentication
10. **GET** `/api/auth/test-admin` - Test admin role
11. **GET** `/api/auth/test-manager` - Test manager role
12. **GET** `/api/auth/test-user` - Test user role
13. **GET** `/api/users/check-permission/{permission}` - Test permission

### **Utility Endpoints:**

14. **GET** `/health` - Health check
15. **GET** `/` - API info
16. **GET** `/swagger` - Swagger UI

## **Testing with CURL:**

### **1. Register a new user:**

**bash**

```
curl -X POST "https://localhost:5001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "password": "Password123",
    "confirmPassword": "Password123"
  }'
```

### **2. Login:**

**bash**

```
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "Admin@123"
  }'
```

### **3. Get current user (with token):**

**bash**

```
curl -X GET "https://localhost:5001/api/users/me" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

### **4. Get all users (Admin only):**

**bash**

```
curl -X GET "https://localhost:5001/api/users" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

## **Testing with Swagger:**

1. Run the application: `dotnet run`
2. Open browser: `https://localhost:5001/swagger`
3. Click "Authorize" button
4. Enter: `Bearer YOUR_TOKEN_HERE`
5. Test all endpoints directly from Swagger UI

## **Pre-seeded Users:**

1. **Admin User:**
   * Email: `admin@example.com`
   * Password: `Admin@123`
   * Roles: `Admin`
   * Permissions: All permissions
2. **Manager User:**
   * Email: `manager@example.com`
   * Password: `Manager@123`
   * Roles: `Manager`
3. **Regular User:**
   * Email: `user@example.com`
   * Password: `User@123`
   * Roles: `User`

### **Successful Login Response:**

**json**

```
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "m5Yt8zqX3vB6w9rS2uV5y8A...",
  "expiresAt": "2024-01-15T10:30:00Z",
  "user": {
    "id": "12345678-1234-1234-1234-123456789012",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "roles": ["User"],
    "permissions": []
  }
}
```

### **Current User Response:**

**json**

```
{
  "id": "12345678-1234-1234-1234-123456789012",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "roles": ["User"],
  "permissions": []
}
```

Your identity server is now complete and ready to use! The API provides full authentication, authorization, role-based permissions, and user management capabilities.

### **1. Get All Roles**

**bash**

```
curl -X GET "https://localhost:5001/api/roles" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

**Response:**

**json**

```
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Admin",
    "description": "Full access administrator",
    "permissions": [
      "users.read",
      "users.create",
      "users.update",
      "users.delete",
      "roles.manage",
      "settings.read",
      "settings.write"
    ]
  },
  {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "name": "Manager",
    "description": "Manager with limited admin access",
    "permissions": [
      "users.read",
      "settings.read"
    ]
  },
  {
    "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
    "name": "User",
    "description": "Regular user",
    "permissions": []
  }
]
```

### **2. Create New Role**

**bash**

```
curl -X POST "https://localhost:5001/api/roles" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Supervisor",
    "description": "Team supervisor with moderate permissions"
  }'
```

**Response (201 Created):**

**json**

```
{
  "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "name": "Supervisor",
  "description": "Team supervisor with moderate permissions",
  "permissions": []
}
```

### **3. Assign Permissions to Role**

**bash**

```
curl -X POST "https://localhost:5001/api/roles/6fa85f64-5717-4562-b3fc-2c963f66afa9/permissions" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "permissions": [
      "users.read",
      "users.update",
      "settings.read"
    ]
  }'
```

**Response:**

**json**

```
{
  "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "name": "Supervisor",
  "description": "Team supervisor with moderate permissions",
  "permissions": [
    "users.read",
    "users.update",
    "settings.read"
  ]
}
```

### **4. Update User Roles**

**bash**

```
curl -X PUT "https://localhost:5001/api/users/123e4567-e89b-12d3-a456-426614174000/roles" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "roles": [
      "User",
      "Supervisor"
    ]
  }'
```

**Response:**

**json**

```
{
  "message": "User roles updated successfully"
}
```

### **5. Get All Permissions**

**bash**

```
curl -X GET "https://localhost:5001/api/roles/permissions" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

**Response:**

**json**

```
[
  {
    "id": "1fa85f64-5717-4562-b3fc-2c963f66afa1",
    "name": "users.read",
    "description": "Read users"
  },
  {
    "id": "2fa85f64-5717-4562-b3fc-2c963f66afa2",
    "name": "users.create",
    "description": "Create users"
  },
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa3",
    "name": "users.update",
    "description": "Update users"
  },
  {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa4",
    "name": "users.delete",
    "description": "Delete users"
  },
  {
    "id": "5fa85f64-5717-4562-b3fc-2c963f66afa5",
    "name": "roles.manage",
    "description": "Manage roles"
  },
  {
    "id": "6fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "settings.read",
    "description": "Read settings"
  },
  {
    "id": "7fa85f64-5717-4562-b3fc-2c963f66afa7",
    "name": "settings.write",
    "description": "Write settings"
  }
]
```

### **6. Get Users by Role**

**bash**

```
curl -X GET "https://localhost:5001/api/roles/Admin/users" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

**Response:**

**json**

```
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "firstName": "Admin",
    "lastName": "User",
    "email": "admin@example.com",
    "roles": ["Admin"],
    "permissions": [
      "users.read",
      "users.create",
      "users.update",
      "users.delete",
      "roles.manage",
      "settings.read",
      "settings.write"
    ]
  }
]
```

### **7. Delete Role**

**bash**

```
curl -X DELETE "https://localhost:5001/api/roles/6fa85f64-5717-4562-b3fc-2c963f66afa9" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

**Success Response:**

**json**

```
{
  "message": "Role deleted successfully"
}
```

**Error Response (if role has users):**

**json**

```
{
  "message": "Cannot delete role that is assigned to users"
}
```

---

## **🎯 Real-World Scenarios**

### **Scenario 1: Create Support Team Role**

**bash**

```
# Step 1: Create Support role
POST /api/roles
{
  "name": "Support",
  "description": "Customer support team"
}

# Step 2: Assign permissions
POST /api/roles/{support-id}/permissions
{
  "permissions": [
    "users.read",
    "users.update",
    "settings.read"
  ]
}

# Step 3: Assign to users
PUT /api/users/{user-id}/roles
{
  "roles": ["Support"]
}
```

### **Scenario 2: Promote User to Manager**

**bash**

```
# Check current roles
GET /api/users/{user-id}

# Update roles
PUT /api/users/{user-id}/roles
{
  "roles": ["User", "Manager"]
}
```

### **Scenario 3: Audit Role Usage**

**bash**

```
# See all users with Admin role
GET /api/roles/Admin/users

# See all permissions
GET /api/roles/permissions

# See role details
GET /api/roles/name/Manager
```

---

## **🔐 Security Rules**

1. **Only Admin** can manage roles
2. **Cannot delete** roles that have users assigned
3. **Role names** must be unique
4. **Permissions** must exist in database
5. **Users must exist** before assigning roles

---

## **📊 Database Seeding (Default Roles & Permissions)**

# Tabble Migrate command

dotnet ef database update --project src/Identity.Infrastructure --startup-project src/Identity.API --context ApplicationDbContext

### **Update SeedData.cs to add more roles:**

**csharp**

```
// Add these in SeedAsync method:

// Add more roles
var supportRole = new Role 
{ 
    Id = Guid.NewGuid(), 
    Name = "Support", 
    Description = "Customer support team" 
};
await _context.Roles.AddAsync(supportRole);

var moderatorRole = new Role 
{ 
    Id = Guid.NewGuid(), 
    Name = "Moderator", 
    Description = "Content moderator" 
};
await _context.Roles.AddAsync(moderatorRole);

// Add more permissions
var additionalPermissions = new List<Permission>
{
    new() { Id = Guid.NewGuid(), Name = "content.create", Description = "Create content" },
    new() { Id = Guid.NewGuid(), Name = "content.edit", Description = "Edit content" },
    new() { Id = Guid.NewGuid(), Name = "content.delete", Description = "Delete content" },
    new() { Id = Guid.NewGuid(), Name = "orders.view", Description = "View orders" },
    new() { Id = Guid.NewGuid(), Name = "orders.manage", Description = "Manage orders" }
};
await _context.Permissions.AddRangeAsync(additionalPermissions);
```

---

## **🛠️ Testing with Postman Collection**

**json**

```
{
  "info": {
    "name": "Role Management API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Login as Admin",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"email\": \"admin@example.com\",\n  \"password\": \"Admin@123\"\n}"
        },
        "url": "{{base_url}}/api/auth/login"
      }
    },
    {
      "name": "Get All Roles",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{admin_token}}"
          }
        ],
        "url": "{{base_url}}/api/roles"
      }
    },
    {
      "name": "Create New Role",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{admin_token}}"
          },
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"name\": \"Auditor\",\n  \"description\": \"System auditor role\"\n}"
        },
        "url": "{{base_url}}/api/roles"
      }
    },
    {
      "name": "Assign Permissions to Role",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{admin_token}}"
          },
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"permissions\": [\n    \"users.read\",\n    \"settings.read\",\n    \"orders.view\"\n  ]\n}"
        },
        "url": "{{base_url}}/api/roles/{{role_id}}/permissions"
      }
    },
    {
      "name": "Update User Roles",
      "request": {
        "method": "PUT",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{admin_token}}"
          },
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"roles\": [\"User\", \"Auditor\"]\n}"
        },
        "url": "{{base_url}}/api/users/{{user_id}}/roles"
      }
    }
  ],
  "variable": [
    {
      "key": "base_url",
      "value": "https://localhost:5001"
    },
    {
      "key": "admin_token",
      "value": ""
    },
    {
      "key": "role_id",
      "value": ""
    },
    {
      "key": "user_id",
      "value": ""
    }
  ]
}
```

---

## **🎉 Summary**
