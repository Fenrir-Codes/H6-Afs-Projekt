For run this application without errors:

#1 - check if your connectionstrings are in use! (if you do not have them, make them, connectionstring to the database showing up in SSMS)
# Connectionstring names should be the same as it written in the program.cs Connectionstrings can be found now in appsettings.json


#2 - Make add-migration -context {DbContext} to the new db contexts if you did not had them earlier. 
# Database contexts are under Data folder (new database context on 04.04.2024 -> Job Offer DbContext)
# (remember : packet manager console!)

#3 - Make update-database -context {DbContext}  (the db contexts you need to update)


#4 Run the application




Make certificate (self signed):

1 - make a folder named "temp" on the root of the c: drive

--------------------------------------------------------------------------------------------------------------------------------------------------------
2 - right click on elevportalen in the project explorer, then click Manage user secrets
    paste in this :

    {
        "CertificatePassword": "SecretPassw0rd#"
    }

    then save it

--------------------------------------------------------------------------------------------------------------------------------------------------------
3 - Open powershell as Admin

    paste in this line and press enter :  $cert = New-SelfSignedCertificate -DnsName elevportalen.dk -CertStoreLocation cert:\LocalMachine\My 

    than this and press enter again : $pwd = ConvertTo-SecureString -String "SecretPassw0rd#" -Force -AsPlainText 

    than the third line and press enter : Export-PfxCertificate -Cert $cert -FilePath C:\temp\elevportalen.pfx -Password $pwd

    now you can find your certificate in c:\temp folder

    then copy it to the root of the project where appsettings.json is

--------------------------------------------------------------------------------------------------------------------------------------------------------
4 - change your host file

    Run Notepad as admin
    click file, open 
    search for : c:\Windows\System32\Drivers\etc
    click show all files, then open the hosts file

    paste in this : 127.0.0.1 elevportalen.dk

    then save the file.

--------------------------------------------------------------------------------------------------------------------------------------------------------
5 - press win+R

    type in : certmgr.msc then press enter

    in the left window choose "Trusted Root Certification Authorities" > "Certificates" 

    left click and "All Tasks" > "Import" 

    follow the wizzard, in the select choose your elevportalen.pfx file from the temp folder you saved it, then click next, if the wizzard ask for password,
    type the password : "SecretPassw0rd#"  then clik next.

    Now the certificate is added to trusted authorities.

    Restart your pc.


--------------------------------------------------------------------------------------------------------------------------------------------------------
6 - Change the progra.cs and launcsettings.json
    
    launchsettings.json is under Properties in the solution explorer
    change the localhost to this under https:

   "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://elevportalen.dk:443;http://elevportalen.dk:80",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },

program.cs file changes :

    add this on the top : using System.Security.Cryptography.X509Certificates;

    add this in the progra.cs file: 



    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ConfigureHttpsDefaults(options =>
        {
        // Grab the secret value from the secret store.
        string? secretValue = builder.Configuration.GetValue<string>("CertificatePassword");
        options.ServerCertificate = new X509Certificate2("elevportalen.pfx", secretValue);
        });
    });



    paste it approx in line 77 but before line var app = builder.Build(); 


    ---------------------------------------------------------------------------------------------------------------------------------------------
    Other:  
    just for make the migration and update easier


    Migrations :

     add-migration -context ApplicationDbContext

     add-migration -context ElevPortalenDataDbContext

     add-migration -context DataRecoveryDbContext

     add-migration -context JobOfferDbContext


    Update:

     update-database -context ApplicationDbContext

     update-database -context ElevPortalenDataDbContext

     update-database -context DataRecoveryDbContext

     update-database -context JobOfferDbContext




     Dropping databases : 

     drop-database -context ApplicationDbContext


     IMPORTANT IF NOTHING WORKS
     Remove Appsettings.Development.Json file