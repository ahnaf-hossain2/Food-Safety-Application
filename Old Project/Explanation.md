# Food Safety Application: Backend-to-Frontend Explanation

## 1. Project overview

This is a C# Windows Forms application targeting `.NET 8 for Windows`. The frontend is made of WinForms forms, controls, buttons, text boxes, radio buttons, combo boxes, list boxes, timers, and message boxes. The backend logic is the database access layer in `DatabaseHelper.cs`, supported by model classes in `Models/` and the shared login state in `SessionManager.cs`.

The application uses SQL Server through the `System.Data.SqlClient` package. The database name is `FoodSafetyDB`, and the connection string is stored in `DatabaseHelper.cs`.

The basic architecture is:

```text
WinForms control
    -> event-handler method
    -> validation and model-object creation
    -> DatabaseHelper method
    -> parameterized SQL command
    -> SQL Server database
    -> model object or success result
    -> controls / MessageBox updated on the form
```

The forms do not write SQL directly. They call a method in `DatabaseHelper`, which keeps database code centralized and keeps UI code focused on input, validation, navigation, and displaying results.

## 2. Application startup and form navigation

`Program.Main()` is the entry point. It enables Windows visual styles, configures compatible text rendering, and opens `Homepage` using `Application.Run(new Homepage())`.

`Homepage` is the first screen and acts as the authentication hub. Forms are connected using event handlers. A button click calls a private method such as `HP_usersignin_pnl_signInBt_Click`. That method reads values from the controls, validates them, calls the appropriate backend method, and then opens or hides forms.

Typical navigation patterns are:

- `Show()` opens another form.
- `Hide()` keeps the current form alive but removes it from view.
- `Close()` closes the current form when returning to another screen.
- `SessionManager` preserves the logged-in user while forms change.
- Logout calls `SessionManager.Logout()` and returns to `Homepage`.

Most visual controls are defined in `.Designer.cs` files. Several management screens also create controls programmatically in their constructors, then retrieve them later by their `Name` property using `this.Controls["ControlName"]`.

## 3. Model layer: the objects moved between UI and database

The model classes provide strongly named properties so forms and database methods can exchange meaningful objects instead of using unstructured values everywhere.

### `FoodSafetyEntity`

This is the abstract base class for `User`, `Food`, `Additive`, `Incident`, and `Complaint`. It provides the shared integer `ID` property. This represents the primary-key value loaded from the database.

### `User`

Properties:

- `ID`
- `FirstName`
- `Username`
- `Gender`
- `Age`
- `Email`
- `Password`
- `Role`

The login query creates a `User` object from a database row. Registration creates a new `User` object from sign-up controls. The role is set to `User` during normal registration and is checked as `Admin` during admin login.

### `Food`

Properties:

- `ID`
- `FoodName`
- `Category`
- `SafetyStatus`

The food model is returned from searches and is used by both the user food-list screen and the admin food-management screen.

### `Additive`

Properties:

- `ID`
- `AdditiveName`
- `Category`
- `INSNumber`
- `MaxLimit`
- `RiskInfo`

Additive records are read from the database and displayed to users and administrators.

### `Complaint`

Properties:

- `ID`
- `UserID`
- `Username`
- `FoodItemName`
- `VendorName`
- `VendorID`
- `DetailComplaint`
- `Status`
- `AdminResponse`
- `DateSubmitted`

A complaint starts with status `Pending`. The user submits it, and an administrator later changes its status and response.

### `Incident`

Properties:

- `ID`
- `Title`
- `Location`
- `IncidentDate`
- `FoodCategory`
- `ViolationType`
- `Status`

Incident records are read and filtered for the incidents archive.

## 4. Session management and the connection between forms

`SessionManager` is a static class that stores the current `User` object in `CurrentUser`.

- `Login(User user)` assigns the authenticated user to `CurrentUser`.
- `IsUserLoggedIn` returns whether `CurrentUser` is not null.
- `Logout()` sets `CurrentUser` to null.

The session is established in `Homepage` after successful authentication. It is then available to `User_Page` and `UserComplaint` without passing the user manually through every form constructor.

For example, complaint submission uses:

```csharp
SessionManager.CurrentUser.ID
SessionManager.CurrentUser.Username
```

This connects the complaint to the database identity of the logged-in user. Complaint history also uses `CurrentUser.ID`, so a user sees only rows whose `UserID` matches the current session.

Admin login uses the same authentication query as user login, but then adds a role check:

```csharp
adminUser != null && adminUser.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
```

Therefore, having valid credentials is not enough for the admin screen; the database record must also have the `Admin` role.

## 5. Detailed user-side data flow

### 5.1 User login

1. The user enters a username and password in controls on `Homepage`.
2. `HP_usersignin_pnl_signInBt_Click` trims the username and password.
3. It rejects empty values before contacting the database.
4. It calls `DatabaseHelper.AuthenticateUser(username, password)`.
5. The helper runs a parameterized query against `Users`.
6. If a row is found, the helper maps that row into a `User` object.
7. `SessionManager.Login(loggedInUser)` stores the object.
8. A new `User_Page` is shown and `Homepage` is hidden.
9. If no row is returned, the password control is cleared and an error message is shown.

### 5.2 Admin login

1. The admin enters credentials in the admin login panel on `Homepage`.
2. The same `AuthenticateUser` method queries the `Users` table.
3. The result is accepted only when `Role` is `Admin`, ignoring case.
4. The admin object is stored in `SessionManager`.
5. `AdminPage` is opened and `Homepage` is hidden.
6. Invalid credentials or a non-admin role produces an error message.

### 5.3 User registration

`User_SignUp` reads first name, username, age, email, password, confirmation password, and gender controls.

Validation occurs before database access:

- Required text fields must not be empty.
- One of the three gender radio buttons must be selected.
- Password and confirmation password must match.
- Age must be numeric.
- Age must be between 13 and 120.
- Email must contain `@` and `.`.

After validation, the form creates:

```csharp
new User(0, firstName, username, gender, age, email, password, "User")
```

The zero ID is intentional because SQL Server generates the new record ID. The form passes this object to `DatabaseHelper.RegisterUser`. On success, it returns to `Homepage`; on failure, it reports that registration failed, commonly because the username may already exist.

### 5.4 Food search and food list

The user can select `Food` or `Additives` in the search panel.

For food:

1. The search text is trimmed.
2. The category radio button must be selected.
3. The query must not be empty.
4. `DatabaseHelper.SearchFoods(query)` is called.
5. SQL searches `FoodName` and `Category` using `LIKE`.
6. Each result row becomes a `Food` object.
7. The form formats the objects into a result message.

When the user selects “check food items,” the form calls `SearchFoods("")`. Because the SQL parameter becomes `%%`, all food rows are returned.

For additives:

1. The form calls `DatabaseHelper.GetAllAdditives()`.
2. All additive rows are mapped into `Additive` objects.
3. The form filters `AdditiveName` in C# with a case-insensitive `IndexOf`.
4. Matching additives are displayed with INS number, maximum limit, and risk information.

This means food filtering is performed in SQL, while additive filtering is currently performed in memory after all additive rows are loaded.

### 5.5 Complaint submission

1. `User_Page` opens `UserComplaint`.
2. The form first checks `SessionManager.IsUserLoggedIn`.
3. It reads username, food item, vendor name, vendor ID, and complaint details.
4. It requires all five form fields to be filled.
5. It creates a `Complaint` object with:
   - ID `0`, allowing the database to generate the ID.
   - The logged-in user's `ID`.
   - The logged-in user's `Username`.
   - The form's food, vendor, and detail values.
   - Status `Pending`.
   - An empty admin response.
6. It calls `DatabaseHelper.SubmitComplaint(complaint)`.
7. The helper inserts the object values into `Complaints`.
8. On success, the form returns to `User_Page`.

Although the form reads a username text box, the object actually uses `SessionManager.CurrentUser.Username`. This is the correct identity source because it ties the complaint to the authenticated session.

### 5.6 Complaint history

1. The user selects the history option.
2. The form verifies that a user is logged in.
3. It calls `GetUserComplaints(SessionManager.CurrentUser.ID)`.
4. SQL selects complaints where `UserID` equals the current user's ID.
5. Rows become `Complaint` objects.
6. The form displays food, vendor, status, details, and any admin response.

This is the main data relationship between the `Users` and `Complaints` tables in the frontend: the current user's ID is used as the filter key.

## 6. Detailed admin-side data flow

### 6.1 Admin dashboard

`AdminPage` provides navigation to food management, additive viewing, complaint management, and user management. Each button opens the corresponding form. Logout clears the static session and returns to `Homepage`.

### 6.2 Food management

`AdminFoodManagement` creates food-management controls programmatically. On load, `RefreshFoodsList` calls `SearchFoods("")` and displays every food.

When the administrator adds food:

1. The form reads food name, category, and selected safety status.
2. All three values are required.
3. The form calls `DatabaseHelper.AddFood(foodName, category, safetyStatus)`.
4. The helper inserts the values into `Foods`.
5. On success, controls are cleared and the list is reloaded from SQL Server.

The form contains a delete button and a `DatabaseHelper.DeleteFood(int foodId)` method exists. However, the current `DeleteBtn_Click` only checks selection, asks for confirmation, and stops at the comment `DELETE FROM DATABASE HERE`. It does not currently identify the selected database ID or call `DeleteFood`. Therefore, deletion is prepared but not completed in the current code.

### 6.3 Complaint management

1. On load, `RefreshComplaintsList` calls `GetAllComplaints`.
2. Each complaint is stored in the form's `complaints` list and represented in a list box.
3. When the selected list-box index changes, the matching `Complaint` object is retrieved from the list.
4. Its current status and response populate the combo box and response text box.
5. On update, the form obtains the selected complaint's ID, the chosen status, and the typed response.
6. It calls `UpdateComplaintStatus(selected.ID, newStatus, newResponse)`.
7. The helper updates `Status` and `AdminResponse` using the complaint ID.
8. The list is refreshed so the new status is visible.

The list-box index is used to locate the corresponding object in the separately stored `complaints` list. The database update itself uses the stable complaint primary key, not the display text.

### 6.4 Additive management

`AdminAdditivesManagement` loads all additive records on form load by calling `GetAllAdditives`. It displays additive name, INS number, and maximum limit.

The current screen is a read-only viewer. Although it creates text boxes labelled additive name, INS number, and max limit, there is no add button handler, no additive insert method in `DatabaseHelper`, and no save operation. Category and risk information are read from existing records but are not editable on this screen.

### 6.5 User management

1. On load, `RefreshUsersList` calls `GetAllUsers`.
2. Every database row is mapped into a `User` object.
3. The list shows ID, username, first name, and role.
4. Search loads all users again and filters usernames in C# using case-insensitive `IndexOf`.
5. Refresh clears the search box and reloads all users.

This is an administrative viewing/search feature; there is no user edit or delete operation.

### 6.6 Incidents archive

`IncidentsArchive` calls `GetIncidents` on load. The helper sorts SQL results by `IncidentDate DESC`, so recent incidents are returned first.

The form then applies the selected status filter in C#:

- `All` displays every incident.
- Other selections display only incidents with the matching status.

The archive is read-only and has no insert, update, or delete operation in the current frontend.

## 7. `DatabaseHelper.cs`: connection and design

`DatabaseHelper` is a `public static` class. Static methods can be called directly by forms without creating a helper object. Its responsibility is database communication and row-to-model mapping.

Each database method generally follows this sequence:

1. Create a `SqlConnection` using the central connection string.
2. Create a SQL query and `SqlCommand`.
3. Add parameters for values supplied by the UI or model.
4. Open the connection.
5. Execute a reader for `SELECT` or `ExecuteNonQuery` for `INSERT`, `UPDATE`, and `DELETE`.
6. Map returned rows to model objects or return a Boolean/result value.
7. Dispose the connection, command, and reader through `using` blocks.

### Connection string field

```csharp
private static string connectionString =
    @"Data Source= DESKTOP-KRCF62\SQLEXPRESS02;Initial Catalog=FoodSafetyDB;Integrated Security=True;";
```

It identifies the SQL Server instance, database, and Windows authentication mode. Because it is private, forms cannot change it directly; all database access is routed through the helper.

### Method 1: `VerifyConnection()`

Purpose: test whether SQL Server can be reached.

Process:

- Creates a connection.
- Calls `Open()`.
- Returns `true` if successful.
- Catches an exception, writes an error to the console, and returns `false`.

Current usage: this method exists as a diagnostic utility, but the current form code does not call it directly. A startup health check could call it before showing database-dependent features.

### Method 2: `AuthenticateUser(string username, string password)`

Purpose: authenticate either a normal user or an administrator.

SQL concept:

```sql
SELECT * FROM Users
WHERE Username = @user AND Password = @pass
```

It uses parameters rather than concatenating text into SQL. If a row is found, it maps the database columns into a `User` object. Nullable `Age` is handled by checking `DBNull.Value`; an empty database age becomes `0` instead of causing a conversion error. No matching row returns `null`.

### Method 3: `SearchFoods(string keyword)`

Purpose: return foods matching a keyword, or all foods when the keyword is empty.

SQL concept:

```sql
SELECT * FROM Foods
WHERE FoodName LIKE @keyword OR Category LIKE @keyword
```

The parameter value is wrapped with `%`, enabling partial matching. Each row becomes a `Food` object containing ID, name, category, and safety status.

### Method 4: `SubmitComplaint(Complaint complaint)`

Purpose: insert a complete complaint record.

It inserts user identity, food, vendor, details, status, admin response, and submission date into `Complaints`.

Important features:

- Accepts one domain object instead of many unrelated arguments.
- Converts null or blank optional values to `DBNull.Value`.
- Defaults blank status to `Pending`.
- Defaults an unset date to `DateTime.Now`.
- Returns `true` when at least one row is inserted.
- Returns `false` if an exception occurs.

### Method 5: `GetAllAdditives()`

Purpose: load the entire `Additives` table.

It maps each row into an `Additive` object containing ID, additive name, category, INS number, maximum limit, and risk information. The user and admin screens perform any additional display filtering after this method returns.

### Method 6: `GetIncidents()`

Purpose: load the incidents archive.

SQL orders records by `IncidentDate DESC`, which places the newest incidents first. Each row is mapped to an `Incident` object. The status filter is applied by the form after retrieval.

### Method 7: `UpdateComplaintStatus(int complaintId, string status, string adminResponse)`

Purpose: allow an administrator to process a complaint.

SQL concept:

```sql
UPDATE Complaints
SET Status = @status, AdminResponse = @response
WHERE ComplaintID = @id
```

The complaint ID identifies the exact record. The method returns `true` if an update affects at least one row; otherwise it returns `false`. It catches database exceptions and converts them into a failure result for the UI.

### Method 7: `CalculateCategorySafetyScore(string category)`

Purpose: calculate the percentage of food records in one category whose safety status is `Safe`.

Process:

1. SQL selects `SafetyStatus` from `Foods` where `Category = @category`.
2. The method counts every returned row as a total item.
3. It increments the safe-item count when the status text is exactly `Safe`.
4. If the category has no records, it returns `0` to avoid division by zero.
5. Otherwise, it returns `safeItems / totalItems * 100`, rounded to two decimal places.

This method provides a percentage rather than a raw count. It is currently a reusable backend calculation, but no current form calls it or displays the score.

### Method 8: `RegisterUser(User user)`

Purpose: insert a new user from the sign-up form.

It inserts first name, username, gender, age, email, password, and role. Optional blank values are converted to `DBNull.Value`. If no role is supplied, it defaults to `User`. The method returns whether the insert affected one or more rows.

### Method 9: `GetUserComplaints(int userId)`

Purpose: retrieve only the complaints belonging to one user.

SQL concept:

```sql
SELECT * FROM Complaints WHERE UserID = @userId
```

The method maps complaint ID, user ID, username, food item, vendor data, details, status, and admin response into `Complaint` objects. It is called by the user history screen with the ID from `SessionManager.CurrentUser`.

### Method 10: `GetAllComplaints()`

Purpose: retrieve every complaint for administration.

It runs `SELECT * FROM Complaints`, maps every row into a `Complaint`, and returns a list used by `AdminComplaintsManagement`.

### Method 11: `AddFood(string foodName, string category, string safetyStatus)`

Purpose: insert a food record from the admin food form.

SQL concept:

```sql
INSERT INTO Foods (FoodName, Category, SafetyStatus)
VALUES (@name, @cat, @status)
```

It returns `true` when the insert succeeds and `false` if it fails.

### Method 12: `DeleteFood(int foodId)`

Purpose: delete a food row by primary key.

SQL concept:

```sql
DELETE FROM Foods WHERE FoodID = @id
```

It is correctly implemented as a parameterized delete method. However, the current admin delete button does not call it, so the method is currently available but not connected to the visible delete workflow.

### Method 13: `GetAllUsers()`

Purpose: load all users for the admin user-management screen.

It maps every `Users` row into a `User` object, including the role. Nullable ages are handled with the same `DBNull.Value` check used by authentication.

## 8. Database safety and resource handling

### Parameterized SQL

The helper uses parameters such as `@user`, `@pass`, `@keyword`, and `@id`. This avoids directly inserting user-entered strings into SQL and reduces SQL injection risk.

### `using` blocks

Connections, commands, and readers are created inside `using` blocks. This ensures resources are disposed even when the operation finishes or an exception occurs.

### `ExecuteReader` versus `ExecuteNonQuery`

- `ExecuteReader()` is used for `SELECT` queries that return rows.
- `ExecuteNonQuery()` is used for inserts, updates, and deletes where the important result is the number of affected rows.

### `DBNull.Value`

Database null is not the same as C# `null`. The helper converts optional blank model properties into `DBNull.Value` before sending them to SQL Server. When reading nullable values such as `Age`, it checks for `DBNull.Value` before conversion.

### Error handling

Write operations such as registration, complaint submission, complaint updates, adding food, and deleting food catch exceptions and return `false`. The form then displays a user-friendly error message.

Read methods currently allow exceptions to propagate to their callers, while `VerifyConnection` reports its exception and returns `false`. A future improvement would use consistent logging and user-facing handling for all database operations.

## 9. Important defense observations and limitations

These points should be explained accurately if asked:

1. **Passwords are stored and compared as plain text.** The current authentication query compares the entered password directly with the database value. Production software should store a salted password hash and verify the hash.
2. **The connection string is hard-coded.** It should normally be moved to secure configuration or a secret store and should not be committed with machine-specific server names.
3. **`VerifyConnection` is available but not called during current startup.** It is useful as a diagnostic method.
4. **`CalculateCategorySafetyScore` exists in `DatabaseHelper.cs` but is not currently called by any form.** It calculates the percentage of foods in a category whose status is exactly `Safe`.
5. **`DeleteFood` exists but the current delete button does not call it.** The UI currently confirms deletion only; it does not delete a row.
6. **Additive management is read-only.** Existing additives can be loaded and searched, but the current screen has no working additive insert/update operation.
7. **Some filtering happens in memory.** Additive and user searches load all rows first and filter them in C#. SQL-side filtering would scale better for large tables.
8. **Results are shown in `MessageBox` and `ListBox` controls.** This is simple for a project demonstration, but a `DataGridView` would be better for sorting, columns, and larger datasets.
9. **Reads are synchronous.** A large query can temporarily block the WinForms UI. Async database methods could improve responsiveness.
10. **There is no visible database schema or migration code in the project.** The SQL Server database and tables must already exist with the expected column names.
11. **The UI does not use a separate service or repository interface.** `DatabaseHelper` is a centralized static data-access class, which is straightforward for this project but harder to mock in automated unit tests.
12. **Role checking is enforced at admin login, but each admin form does not independently repeat the role check.** A stronger production design would enforce authorization at every protected operation as well.

## 10. Short defense script

A concise explanation for the panel is:

> The application uses a layered WinForms design. The forms collect input through controls and handle button events. Before any database call, the event handler validates the values and either creates a model object or passes individual values to `DatabaseHelper`. `DatabaseHelper` is the centralized data-access layer: it opens a SQL Server connection, executes parameterized commands, maps result rows into model objects, and returns lists or Boolean success values. After a successful login, the returned `User` object is stored in the static `SessionManager`, so later forms can use the logged-in user's ID. That ID links complaint submission and complaint history to the correct user. Admin login uses the same authentication method but checks that the returned role is `Admin`. The admin forms reload model lists after changes, so the frontend reflects the database state.

If asked specifically about `DatabaseHelper`, say:

> It contains one connection configuration, one connection-test method, authentication, registration, food search and insert/delete operations, additive and incident retrieval, complaint insert/retrieval/update operations, a category safety-score calculation, and user retrieval. All SQL values supplied by the UI are parameters, and database resources are disposed with `using` blocks.

## 11. Feature-to-method quick reference

| Frontend feature | Form/event | Backend method |
|---|---|---|
| User login | `Homepage.HP_usersignin_pnl_signInBt_Click` | `AuthenticateUser` |
| Admin login | `Homepage.Hp_adminPnlSigninBt_Click` | `AuthenticateUser` + role check |
| Registration | `User_SignUp.SignUp_pageCreateAccBT_Click` | `RegisterUser` |
| Search foods | `User_Page.UP_searchpnl_searchBt_Click` | `SearchFoods` |
| View all foods | `User_Page.U_Page_CheckFoodItemsBt_Click` | `SearchFoods("")` |
| Search additives | `User_Page.UP_searchpnl_searchBt_Click` | `GetAllAdditives` + C# filter |
| Submit complaint | `UserComplaint.ComplaintPage_submitBt_Click` | `SubmitComplaint` |
| User complaint history | `User_Page.U_Page_HistoryBt_Click` | `GetUserComplaints` |
| Add food | `AdminFoodManagement.AddBtn_Click` | `AddFood` |
| Delete food | `AdminFoodManagement.DeleteBtn_Click` | `DeleteFood` exists, but is not currently called |
| Admin complaint list | `AdminComplaintsManagement.RefreshComplaintsList` | `GetAllComplaints` |
| Update complaint | `AdminComplaintsManagement.UpdateBtn_Click` | `UpdateComplaintStatus` |
| View additives | `AdminAdditivesManagement.AdminAdditivesManagement_Load` | `GetAllAdditives` |
| View/search users | `AdminUsersManagement` | `GetAllUsers` + C# filter |
| View/filter incidents | `IncidentsArchive` | `GetIncidents` |
| Test database connection | Diagnostic use | `VerifyConnection` exists but is not called by current forms |
| Calculate category score | No current form call | `CalculateCategorySafetyScore` exists but is not currently used |

## 12. Final data-flow summary

The important link is:

```text
Control values
    -> event handler validation
    -> User/Food/Complaint/etc. model
    -> DatabaseHelper
    -> parameterized SQL
    -> database row(s)
    -> model list/object or Boolean
    -> MessageBox/ListBox/form navigation
```

The frontend therefore acts as the presentation and interaction layer, the model classes represent application data, `DatabaseHelper` performs persistence, and `SessionManager` carries authenticated user context between forms.
