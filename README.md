<p align="center">
  <img src="https://img.shields.io/badge/C%23-0078D7?logo=csharp&amp;logoColor=white&amp;style=for-the-badge">
  <img src="https://img.shields.io/badge/.NET%20Framework-512BD4?logo=dotnet&amp;logoColor=white&amp;style=for-the-badge">
</p>

# File Monitoring & Management Windows Service

## 📌 Overview

A Windows Service that monitors a directory, processes new files, and logs activity for traceability.

---

## ✔ Features

- Detects new files added to the monitored directory
- Renames each new file using a GUID
- Moves the processed file to a destination folder
- Logs all events (e.g., Service Started, File Detected, File Moved)
- Logs errors & runtime activity to both:
  - A log file
  - Windows Event Viewer
- Developed using Visual Studio 2022, C#, Windows Service (.NET Framework).

---

## 🧪 Running the Service (Two Modes)

You can run the project in either **Console Mode (development)** or **Service Mode (Windows service installation)**.

### 🖥️ Console Mode (Recommended for Testing)

1. Open Visual Studio 2022 (ensure the .NET Desktop Development workload is installed).
2. Create a new project: Windows Service (.NET Framework).
3. Delete the default template files and add this project to your solution.
4. Build and run the project in Release mode → the service runs in console form.

### ⚙️ Service Mode (Install as Windows Service)

1. In Visual Studio, right-click the project → Open Folder in File Explorer.
2. Navigate to:
   - `bin` → `Release`
   - Copy all generated files, including the `.exe`.
3. Create a folder on *C:* (e.g., `C:\FileService`) and paste the files.
4. Open CMD (Run as Administrator).
5. Navigate to the folder:
   ```
   cd "C:\FileService"
   ```
6. Use InstallUtil.exe to install the service:

   **32-bit:**
   ```
   C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe FileManagementService.exe
   ```

   **64-bit:**
   ```
   C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe FileManagementService.exe
   ```

7. If installation succeeds, you will see a success message.
8. Open Services (Service Control Manager) → find your service → Start/Stop it manually.

---

## 📝 Logging

Logs are written to:

- A dedicated log file
- Windows Event Viewer (Application log)

---

## 📂 Configuration

Update your directory paths in `app.config`:

```xml
<appSettings>
  <add key="SourceFolder" value="C:\SourceFolder" />
  <add key="DestinationFolder" value="C:\DestinationFolder" />
  <add key="LogDirectory" value="C:\Logs" />
</appSettings>
```
