# This installs two files, app.exe and logo.ico, creates a start menu shortcut, builds an uninstaller, and
# adds uninstall information to the registry for Add/Remove Programs
 
# To get started, put this script into a folder with the two files (app.exe, logo.ico, and license.rtf -
# You'll have to create these yourself) and run makensis on it
 
# If you change the names "app.exe", "logo.ico", or "license.rtf" you should do a search and replace - they
# show up in a few places.
# All the other settings can be tweaked by editing the !defines at the top of this script
!define APPNAME "DogeDashCoinCalculator"
!define COMPANYNAME "DogeDashCoinCalculator"
#!define DESCRIPTION "Control how many DogeDash coins (www.dogedash.com) you spend."
# These three must be integers
#!define VERSIONMAJOR 1
#!define VERSIONMINOR 1
#!define VERSIONBUILD 1
# These will be displayed by the "Click here for support information" link in "Add/Remove Programs"
# It is possible to use "mailto:" links in here to open the email client
#!define HELPURL "http://..." # "Support Information" link
#!define UPDATEURL "http://..." # "Product Updates" link
#!define ABOUTURL "http://..." # "Publisher" link
# This is the size (in kB) of all the files copied into "Program Files"
!define INSTALLSIZE 162105
 
RequestExecutionLevel admin ;Require admin rights on NT6+ (When UAC is turned on)
 
InstallDir "$PROGRAMFILES64\${APPNAME}"
 
# rtf or txt file - remember if it is txt, it must be in the DOS text format (\r\n)
#LicenseData "license.rtf"
# This will be in the installer/uninstaller's title bar
Name "DogeDash Coin Calculator"
Icon "icon.ico"
outFile "dogedashcoincalculator_install.exe"
 
!include LogicLib.nsh
 
# Just three pages - license agreement, install location, and installation
#page license
page directory
Page instfiles
 
 
!macro VerifyUserIsAdmin
UserInfo::GetAccountType
pop $0
${If} $0 != "admin" ;Require admin rights on NT4+
        messageBox mb_iconstop "Administrator rights required!"
        setErrorLevel 740 ;ERROR_ELEVATION_REQUIRED
        quit
${EndIf}
!macroend
 
function .onInit
	setShellVarContext all
	!insertmacro VerifyUserIsAdmin
functionEnd
 
section "install"
	# Files for the install directory - to build the installer, these should be in the same directory as the install script (this file)
	setOutPath $INSTDIR
	# Files added here should be removed by the uninstaller (see section "uninstall")
	file "Show_Invested_Coins.exe"
	file "icon.ico"
	file "D3DCompiler_47_cor3.dll"
	file "PenImc_cor3.dll"
	file "PresentationNative_cor3.dll"
	file "vcruntime140_cor3.dll"
	file "wpfgfx_cor3.dll"
	# Add any other files for the install directory (license files, app data, etc) here
 
	# Uninstaller - See function un.onInit and section "uninstall" for configuration
	writeUninstaller "$INSTDIR\uninstall.exe"
 
	# Start Menu
	createDirectory "$SMPROGRAMS\${APPNAME}"
	createShortCut "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk" "$INSTDIR\Show_Invested_Coins.exe" "" "$INSTDIR\icon.ico"
	createShortCut "$SMPROGRAMS\${APPNAME}\uninstall.lnk" "$INSTDIR\uninstall.exe"
	
	# Desktop
	createShortCut "$DESKTOP\${APPNAME}.lnk" "$INSTDIR\Show_Invested_Coins.exe" "" "$INSTDIR\icon.ico"
 
	# Registry information for add/remove programs
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME} (remove only)"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$INSTDIR\uninstall.exe"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayIcon" "$INSTDIR\icon.ico"
  
	# There is no option for modifying or repairing the install
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoModify" 1
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoRepair" 1
	# Set the INSTALLSIZE constant (!defined at the top of this script) so Add/Remove Programs can accurately report the size
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "EstimatedSize" ${INSTALLSIZE}
sectionEnd
 
# Uninstaller
 
function un.onInit
	SetShellVarContext all
 
	#Verify the uninstaller - last chance to back out
	MessageBox MB_OKCANCEL "Permanantly remove ${APPNAME}?" IDOK next
		Abort
	next:
	!insertmacro VerifyUserIsAdmin
functionEnd
 
section "uninstall"
 
	# Remove Start Menu launcher
	delete "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk"
	delete "$SMPROGRAMS\${APPNAME}\uninstall.lnk"
	# Try to remove the Start Menu folder - this will only happen if it is empty
	rmDir "$SMPROGRAMS\${APPNAME}"
 
	# Remove files
	delete $INSTDIR\Show_Invested_Coins.exe
	delete $INSTDIR\icon.ico
	delete $INSTDIR\D3DCompiler_47_cor3.dll
	delete $INSTDIR\PenImc_cor3.dll
	delete $INSTDIR\PresentationNative_cor3.dll
	delete $INSTDIR\vcruntime140_cor3.dll
	delete $INSTDIR\wpfgfx_cor3.dll
	SetShellVarContext current
	delete $LOCALAPPDATA\DogeDashCoins\dd_coins
	delete $LOCALAPPDATA\DogeDashCoins\dd_config
	rmDir $LOCALAPPDATA\DogeDashCoins
	SetShellVarContext all
	delete "$DESKTOP\${APPNAME}.lnk"
 
	# Always delete uninstaller as the last action
	delete $INSTDIR\uninstall.exe
 
	# Try to remove the install directory - this will only happen if it is empty
	setOutPath "$PROGRAMFILES64"
	# Only works if the actual dir is not the installDir, so change it like above
	rmDir $INSTDIR
 
	# Remove uninstaller information from the registry
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"

sectionEnd