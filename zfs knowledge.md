# Usage

To use this Tool, openzfs has to be installed. On windows, use OpenZfs port for Windows

# zpool

## General

### Acquire diskdrives

To get the available disk drives that can host a zpool, start powershell and type:
```
Get-WmiObject Win32_DiskDrive
```
Since zfs and zpool requires admin privileges, start powershell as admin or use the sudo command (can be installed through winget).

## Creation

To create a zpool on a drive the following command can be used:
```
zpool create -O casesensitivity=insensitive -O normalization=formD -O atime=off -o ashift=12 <pool name> <drive name>
```
PHYSICALDRIVE4 is the disk i used in place of <drive name>.
<pool name> describes the name of the pool to be used. 

## import

### list file based pools from directory

list pools that can be imported from library:
```
zpool import -d <directory path>      # No filename here, only directory
```

pools are then listed in this way:

```
working on dev '<path>\<filename>'
Assuming 512 byte sector size
correcting path: '//./<path>/<filename>'
  pool: <pool name>
    id: <pool id>
 state: ONLINE
action: The pool can be imported using its name or numeric identifier.
config:

        <pool Name>                                    ONLINE
          //./<full path>                              ONLINE
```

### Import file based pool

```
zpool import -d <directory> <pool name>
```

Output should be

```
working on dev '<directory>\<filename>'
Assuming 512 byte sector size
correcting path: '//./<full path>'
sending mountpoint: '\??\?:'
```

At this point, the mounted folder should be accessible through windows explorer