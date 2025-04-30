# Resonite REST API
This plugin provides a REST-API for Resonite to serve as a foundation for integrations with external software. It is designed to be a direct representation of Resonites internal data structures with minimal abstraction layers. This API is implemented as a userspace-core plugin, meaning it will not break network compatibility, as the added component is only available in userspace.

## How it works
When Resonite is started with the plugin installed, a non-persistant Slot called "Resonite API" will be added to the root of the clients userspace, with the `ResoniteApi` component attached to it. This component hosts the server for the REST API. The API is then accessible via HTTP on the specified hostname & port (default is `localhost:4600`).

## Security
To prevent access to sensitive API-endpoints from within Resonite, most endpoints are not accessible if the user-agent accessing it is Resonite itself.

Besides checking the user agent, there is **no authentication required** to access the API endpoints. **DO NOT** expose the API directly to the internet. It is up to you to ensure that the API is only accessible for your integrated external software.

## Configuration
The API can be configured through the following launch arguments:

| Launch Argument                    | Description                                        |
|------------------------------------|----------------------------------------------------|
| `--ResoniteApiHostname [Hostname]` | Specifies the API hostname (default: `localhost`). |
| `--ResoniteApiPort [port]`         | Specifies the API port (default: `4600`).          |

Note for use on Windows: The API server uses a wildcard HTTP listener on the configured address, which on Windows requires to be added to the HTTP access control list (acl) first. When the address isn't already allowed, **a UAC dialog will ask for permission to add the address to the ACL** on first launch.

## Resources
Resources are data entities that are accessible via HTTP-endpoints within the API. Some are read-only, others can be partially or fully modified. There are a few quality-of-life features that are usable for all resource types:

#### Resource list filtering
All lists of Resources can be filtered by adding query parameters to the request URL. The name of the query parameter represents the path of the property to filter by, and the value represents the (JSON-)value to filter for. The API will return all items that match the filter. The value can be prefixed by one of the following filter operators:

| Operator          | Description                                                                         |
|-------------------|-------------------------------------------------------------------------------------|
| `~null~`          | Matches resources where the value is null.                                          |
| `~notnull~`       | Matches resources where the value is not null.                                      |
| `~eq~ [value]`    | Matches resources where the value is equal to the comparison value. (Default)       |
| `~noteq~ [value]` | Matches resources where the value is not equal to the comparison value.             |
| `~lt~ [value]`    | Matches resources where the value is less than the comparison value.                |
| `~lteq~ [value]`  | Matches resources where the value is less than or equal to the comparison value.    |
| `~gt~ [value]`    | Matches resources where the value is greater than the comparison value.             |
| `~gteq~ [value]`  | Matches resources where the value is greater than or equal to the comparison value. |

##### Example:
Filter list of all contacts that are not accepted: `GET /ResoniteApi/contacts/?contactStatus=~noteq~"Accepted"`

#### Resource sub-selecting
Sometimes you are only interested in a part of a resource. In those cases, you can instruct the API to only return the part of the resource you are interested in by adding the path to the element your're looking for to the end of the request URL.

Note that this only works for endpoints that return a single resource.

##### Example:
Select only the `iconUrl` property of a users `profile` property: ` GET /ResoniteApi/users/U-JackTheFoxOtter/profile/iconUrl`

## Supported Resources & Resource Methods

### Users
Allows searching for users and retrieving user information.

| Method                                     | Description                                      |
|--------------------------------------------|--------------------------------------------------|
| `GET /ResoniteApi/users?searchName={name}` | Search for users using the provided search name. |
| `GET /ResoniteApi/users/{id}`              | Get user by ID.                                  |

### Contacts
Provides read & write access to the clients contact list.

| Method                              | Description                                    |
|-------------------------------------|------------------------------------------------|
| `GET /ResoniteApi/contacts`         | Get list of all contacts.                      |
| `GET /ResoniteApi/contacts/{id}`    | Get contact by ID.                             |
| `POST /ResoniteApi/contacts`        | Create a new contact. (Send a contact request) |
| `PATCH /ResoniteApi/contacts/{id}`  | Update an existing contact by id.              |
| `DELETE /ResoniteApi/contacts/{id}` | Delete contact by ID. (Remove contact)*        |

(* Note: Deleting doesn't actually delete the contact resource, it updates its contact status to `"None"`.)

### Cloud Variables
Provides read & write access to cloud variable definitions & values.
Note that the cloud variables permissions apply.

...