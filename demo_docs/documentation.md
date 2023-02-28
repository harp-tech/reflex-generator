

# Device

<font size="4"> - device: ExampleDevice </font> 

<font size="4"> - whoAmI: 0 </font> 

<font size="4"> - firmwareVersion: 0.1 </font> 

<font size="4"> - hardwareTargets: 0.1 </font> 

<font size="4"> - architecture: atmega </font> 


--------

# Registers

## Summary table
| name                                                              |   address | payloadType                 |   payloadLength | registerType                         | payloadSpec                                                                              | maskType                      | description                                                           | converter   | defaultValue   | maxValue   | minValue   | interfaceType   | visibility                           | group   |
|:------------------------------------------------------------------|----------:|:----------------------------|----------------:|:-------------------------------------|:-----------------------------------------------------------------------------------------|:------------------------------|:----------------------------------------------------------------------|:------------|:---------------|:-----------|:-----------|:----------------|:-------------------------------------|:--------|
| [Cam0Event](#ref-Device-Register-Cam0Event)                       |        32 | [U8](#ref-PayloadType-U8)   |               1 | [Event](#ref-RegisterType-Event)     |                                                                                          |                               | None                                                                  |             |                |            |            |                 | [Public](#ref-VisibilityType-Public) | None    |
| [Cam0TriggerFrequency](#ref-Device-Register-Cam0TriggerFrequency) |        33 | [U16](#ref-PayloadType-U16) |               1 | [Command](#ref-RegisterType-Command) |                                                                                          | ['[DO](#ref-Device-Mask-DO)'] | Sets the trigger frequency for camera 0 between 1 and 1000.           |             |                |            |            |                 | [Public](#ref-VisibilityType-Public) | None    |
| [Cam0TriggerDuration](#ref-Device-Register-Cam0TriggerDuration)   |        34 | [U16](#ref-PayloadType-U16) |               1 | [Command](#ref-RegisterType-Command) |                                                                                          |                               | Sets the duration of the trigger pulse (minimum is 100) for camera 0. |             |                |            |            |                 | [Public](#ref-VisibilityType-Public) | None    |
| [StartAndStop](#ref-Device-Register-StartAndStop)                 |        35 | [U8](#ref-PayloadType-U8)   |               1 | [Command](#ref-RegisterType-Command) |                                                                                          |                               | Starts or stops the camera immediately.                               |             |                |            |            |                 | [Public](#ref-VisibilityType-Public) | None    |
| [InState](#ref-Device-Register-InState)                           |        36 | [U8](#ref-PayloadType-U8)   |               1 | [Event](#ref-RegisterType-Event)     |                                                                                          |                               | Contains the state of the input ports.                                |             |                |            |            |                 | [Public](#ref-VisibilityType-Public) | None    |
| [Valve0Pulse](#ref-Device-Register-Valve0Pulse)                   |        37 | [U8](#ref-PayloadType-U8)   |               1 | [Command](#ref-RegisterType-Command) |                                                                                          |                               | Configures the valve 0 open time in milliseconds.                     |             |                |            |            |                 | [Public](#ref-VisibilityType-Public) | None    |
| [OutSet](#ref-Device-Register-OutSet)                             |        38 | [U8](#ref-PayloadType-U8)   |               1 | [Command](#ref-RegisterType-Command) |                                                                                          |                               | Bitmask to set the available outputs.                                 |             |                |            |            |                 | [Public](#ref-VisibilityType-Public) | None    |
| [OutClear](#ref-Device-Register-OutClear)                         |        39 | [U8](#ref-PayloadType-U8)   |               1 | [Command](#ref-RegisterType-Command) |                                                                                          |                               | Bitmask to clear the available outputs.                               |             |                |            |            |                 | [Public](#ref-VisibilityType-Public) | None    |
| [OutToggle](#ref-Device-Register-OutToggle)                       |        40 | [U8](#ref-PayloadType-U8)   |               1 | [Command](#ref-RegisterType-Command) |                                                                                          |                               | Bitmask to toggle the available outputs.                              |             |                |            |            |                 | [Public](#ref-VisibilityType-Public) | None    |
| [OutWrite](#ref-Device-Register-OutWrite)                         |        41 | [U8](#ref-PayloadType-U8)   |               1 | [Command](#ref-RegisterType-Command) |                                                                                          | ['[DO](#ref-Device-Mask-DO)'] | Bitmask to write the available outputs.                               |             |                |            |            |                 | [Public](#ref-VisibilityType-Public) | None    |
| [AnalogIn](#ref-Device-Register-AnalogIn)                         |        42 | [U16](#ref-PayloadType-U16) |               2 | [Event](#ref-RegisterType-Event)     | ['[ADC](#ref-Device-PayloadMember-ADC)', '[Encoder](#ref-Device-PayloadMember-Encoder)'] |                               | None                                                                  |             |                |            |            |                 | [Public](#ref-VisibilityType-Public) | None    |

## Technical documentation
### <a name="ref-Device-Register-Cam0Event"></a>Cam0Event
> address = 32 

> payloadType = [U8](#ref-PayloadType-U8) 

> payloadLength = 1 

> registerType = [Event](#ref-RegisterType-Event) 

> payloadSpec = None 

> maskType = None 

> description = None 

> converter = None 

> defaultValue = None 

> maxValue = None 

> minValue = None 

> interfaceType = None 

> visibility = [Public](#ref-VisibilityType-Public) 

> group = None 

### <a name="ref-Device-Register-Cam0TriggerFrequency"></a>Cam0TriggerFrequency
> address = 33 

> payloadType = [U16](#ref-PayloadType-U16) 

> payloadLength = 1 

> registerType = [Command](#ref-RegisterType-Command) 

> payloadSpec = None 

> maskType = [[DO](#ref-Device-Mask-DO)] 

> description = Sets the trigger frequency for camera 0 between 1 and 1000. 

> converter = None 

> defaultValue = None 

> maxValue = None 

> minValue = None 

> interfaceType = None 

> visibility = [Public](#ref-VisibilityType-Public) 

> group = None 

### <a name="ref-Device-Register-Cam0TriggerDuration"></a>Cam0TriggerDuration
> address = 34 

> payloadType = [U16](#ref-PayloadType-U16) 

> payloadLength = 1 

> registerType = [Command](#ref-RegisterType-Command) 

> payloadSpec = None 

> maskType = None 

> description = Sets the duration of the trigger pulse (minimum is 100) for camera 0. 

> converter = None 

> defaultValue = None 

> maxValue = None 

> minValue = None 

> interfaceType = None 

> visibility = [Public](#ref-VisibilityType-Public) 

> group = None 

### <a name="ref-Device-Register-StartAndStop"></a>StartAndStop
> address = 35 

> payloadType = [U8](#ref-PayloadType-U8) 

> payloadLength = 1 

> registerType = [Command](#ref-RegisterType-Command) 

> payloadSpec = None 

> maskType = None 

> description = Starts or stops the camera immediately. 

> converter = None 

> defaultValue = None 

> maxValue = None 

> minValue = None 

> interfaceType = None 

> visibility = [Public](#ref-VisibilityType-Public) 

> group = None 

### <a name="ref-Device-Register-InState"></a>InState
> address = 36 

> payloadType = [U8](#ref-PayloadType-U8) 

> payloadLength = 1 

> registerType = [Event](#ref-RegisterType-Event) 

> payloadSpec = None 

> maskType = None 

> description = Contains the state of the input ports. 

> converter = None 

> defaultValue = None 

> maxValue = None 

> minValue = None 

> interfaceType = None 

> visibility = [Public](#ref-VisibilityType-Public) 

> group = None 

### <a name="ref-Device-Register-Valve0Pulse"></a>Valve0Pulse
> address = 37 

> payloadType = [U8](#ref-PayloadType-U8) 

> payloadLength = 1 

> registerType = [Command](#ref-RegisterType-Command) 

> payloadSpec = None 

> maskType = None 

> description = Configures the valve 0 open time in milliseconds. 

> converter = None 

> defaultValue = None 

> maxValue = None 

> minValue = None 

> interfaceType = None 

> visibility = [Public](#ref-VisibilityType-Public) 

> group = None 

### <a name="ref-Device-Register-OutSet"></a>OutSet
> address = 38 

> payloadType = [U8](#ref-PayloadType-U8) 

> payloadLength = 1 

> registerType = [Command](#ref-RegisterType-Command) 

> payloadSpec = None 

> maskType = None 

> description = Bitmask to set the available outputs. 

> converter = None 

> defaultValue = None 

> maxValue = None 

> minValue = None 

> interfaceType = None 

> visibility = [Public](#ref-VisibilityType-Public) 

> group = None 

### <a name="ref-Device-Register-OutClear"></a>OutClear
> address = 39 

> payloadType = [U8](#ref-PayloadType-U8) 

> payloadLength = 1 

> registerType = [Command](#ref-RegisterType-Command) 

> payloadSpec = None 

> maskType = None 

> description = Bitmask to clear the available outputs. 

> converter = None 

> defaultValue = None 

> maxValue = None 

> minValue = None 

> interfaceType = None 

> visibility = [Public](#ref-VisibilityType-Public) 

> group = None 

### <a name="ref-Device-Register-OutToggle"></a>OutToggle
> address = 40 

> payloadType = [U8](#ref-PayloadType-U8) 

> payloadLength = 1 

> registerType = [Command](#ref-RegisterType-Command) 

> payloadSpec = None 

> maskType = None 

> description = Bitmask to toggle the available outputs. 

> converter = None 

> defaultValue = None 

> maxValue = None 

> minValue = None 

> interfaceType = None 

> visibility = [Public](#ref-VisibilityType-Public) 

> group = None 

### <a name="ref-Device-Register-OutWrite"></a>OutWrite
> address = 41 

> payloadType = [U8](#ref-PayloadType-U8) 

> payloadLength = 1 

> registerType = [Command](#ref-RegisterType-Command) 

> payloadSpec = None 

> maskType = [[DO](#ref-Device-Mask-DO)] 

> description = Bitmask to write the available outputs. 

> converter = None 

> defaultValue = None 

> maxValue = None 

> minValue = None 

> interfaceType = None 

> visibility = [Public](#ref-VisibilityType-Public) 

> group = None 

### <a name="ref-Device-Register-AnalogIn"></a>AnalogIn
> address = 42 

> payloadType = [U16](#ref-PayloadType-U16) 

> payloadLength = 2 

> registerType = [Event](#ref-RegisterType-Event) 

> payloadSpec = [PayloadMember(name='ADC', mask=None, offset=0, maskType=None, description='The value of the board ADC.', converter=None, defaultValue=None, maxValue=None, minValue=None, interfaceType=None, uid=ref-Device-PayloadMember-ADC), PayloadMember(name='Encoder', mask=None, offset=1, maskType=None, description='The value of the quadrature counter in Port 2.', converter=None, defaultValue=None, maxValue=None, minValue=None, interfaceType=None, uid=ref-Device-PayloadMember-Encoder)] 

> maskType = None 

> description = None 

> converter = None 

> defaultValue = None 

> maxValue = None 

> minValue = None 

> interfaceType = None 

> visibility = [Public](#ref-VisibilityType-Public) 

> group = None 


--------

# BitMasks

## Summary table
| name                      | description                                            | value   | bits           | maskCategory                         |
|:--------------------------|:-------------------------------------------------------|:--------|:---------------|:-------------------------------------|
| [DO](#ref-Device-Mask-DO) | Bitmask representing the state of the digital outputs. |         | ['0x1', '0x2'] | [BitMask](#ref-MaskCategory-BitMask) |

## Technical documentation
### <a name="ref-Device-Mask-DO"></a>DO
> description = Bitmask representing the state of the digital outputs. 

> bits = 


 * *DO0*

        	value = 0x1

        	description = The state of digital output pin 0.

 * *DO1*

        	value = 0x2

        	description = The state of digital output pin 1.

--------
# PayloadMembers

## Summary table
| name                                         | mask   |   offset | maskType   | description                                    | converter   | defaultValue   | maxValue   | minValue   | interfaceType   |
|:---------------------------------------------|:-------|---------:|:-----------|:-----------------------------------------------|:------------|:---------------|:-----------|:-----------|:----------------|
| [ADC](#ref-Device-PayloadMember-ADC)         |        |        0 |            | The value of the board ADC.                    |             |                |            |            |                 |
| [Encoder](#ref-Device-PayloadMember-Encoder) |        |        1 |            | The value of the quadrature counter in Port 2. |             |                |            |            |                 |

## Technical documentation
### <a name="ref-Device-PayloadMember-ADC"></a>ADC
> mask = None 

> offset = 0 

> maskType = None 

> description = The value of the board ADC. 

> converter = None 

> defaultValue = None 

> maxValue = None 

> minValue = None 

> interfaceType = None 

### <a name="ref-Device-PayloadMember-Encoder"></a>Encoder
> mask = None 

> offset = 1 

> maskType = None 

> description = The value of the quadrature counter in Port 2. 

> converter = None 

> defaultValue = None 

> maxValue = None 

> minValue = None 

> interfaceType = None 



# IOs
| name                                   | port   |   pinNumber | direction                           | pinMode                                    | triggerMode                           | interruptPriority                     |   interruptNumber | description                         |   allowRead | initialState                     |   invert |
|:---------------------------------------|:-------|------------:|:------------------------------------|:-------------------------------------------|:--------------------------------------|:--------------------------------------|------------------:|:------------------------------------|------------:|:---------------------------------|---------:|
| [Lick0](#ref-Device-InputPin-Lick0)    | PORTA  |           0 | [input](#ref-DirectionType-input)   | [tristate](#ref-InputPinModeType-tristate) | [toggle](#ref-TriggerModeType-toggle) | [low](#ref-InterruptPriorityType-low) |                 0 | Input for the Lick 0.               |         nan | nan                              |      nan |
| [Lick1](#ref-Device-InputPin-Lick1)    | PORTA  |           1 | [input](#ref-DirectionType-input)   | [tristate](#ref-InputPinModeType-tristate) | [toggle](#ref-TriggerModeType-toggle) | [low](#ref-InterruptPriorityType-low) |                 1 | Input for the Lick 1.               |         nan | nan                              |      nan |
| [Valve0](#ref-Device-OutputPin-Valve0) | PORTA  |           6 | [output](#ref-DirectionType-output) | [wiredOr](#ref-OutputPinModeType-wiredOr)  | nan                                   | nan                                   |               nan | Controls the transistor of Valve 0. |           0 | [low](#ref-InitialStateType-low) |        0 |

## Technical documentation
### <a name="ref-Device-InputPin-Lick0"></a>Lick0
> port = PORTA 

> pinNumber = 0 

> direction = [input](#ref-DirectionType-input) 

> pinMode = [tristate](#ref-InputPinModeType-tristate) 

> triggerMode = [toggle](#ref-TriggerModeType-toggle) 

> interruptPriority = [low](#ref-InterruptPriorityType-low) 

> interruptNumber = 0 

> description = Input for the Lick 0. 

### <a name="ref-Device-InputPin-Lick1"></a>Lick1
> port = PORTA 

> pinNumber = 1 

> direction = [input](#ref-DirectionType-input) 

> pinMode = [tristate](#ref-InputPinModeType-tristate) 

> triggerMode = [toggle](#ref-TriggerModeType-toggle) 

> interruptPriority = [low](#ref-InterruptPriorityType-low) 

> interruptNumber = 1 

> description = Input for the Lick 1. 

### <a name="ref-Device-OutputPin-Valve0"></a>Valve0
> port = PORTA 

> pinNumber = 6 

> direction = [output](#ref-DirectionType-output) 

> allowRead = False 

> pinMode = [wiredOr](#ref-OutputPinModeType-wiredOr) 

> initialState = [low](#ref-InitialStateType-low) 

> invert = False 

> description = Controls the transistor of Valve 0. 




## References
### <a name="ref-PayloadType-PayloadType"></a>PayloadType
- <a name="ref-PayloadType-U8"></a>U8

- <a name="ref-PayloadType-U16"></a>U16

- <a name="ref-PayloadType-U32"></a>U32

- <a name="ref-PayloadType-U64"></a>U64

- <a name="ref-PayloadType-S8"></a>S8

- <a name="ref-PayloadType-S16"></a>S16

- <a name="ref-PayloadType-S32"></a>S32

- <a name="ref-PayloadType-S64"></a>S64

- <a name="ref-PayloadType-Float"></a>Float



### <a name="ref-RegisterType-RegisterType"></a>RegisterType
- <a name="ref-RegisterType-NONE"></a>NONE

- <a name="ref-RegisterType-Command"></a>Command

- <a name="ref-RegisterType-Event"></a>Event

- <a name="ref-RegisterType-Config"></a>Config

- <a name="ref-RegisterType-Both"></a>Both



### <a name="ref-VisibilityType-VisibilityType"></a>VisibilityType
- <a name="ref-VisibilityType-Public"></a>Public

- <a name="ref-VisibilityType-Private"></a>Private



### <a name="ref-MaskCategory-MaskCategory"></a>MaskCategory
- <a name="ref-MaskCategory-BitMask"></a>BitMask

- <a name="ref-MaskCategory-GroupMask"></a>GroupMask



### <a name="ref-DirectionType-DirectionType"></a>DirectionType
- <a name="ref-DirectionType-input"></a>input

- <a name="ref-DirectionType-output"></a>output



### <a name="ref-InputPinModeType-InputPinModeType"></a>InputPinModeType
- <a name="ref-InputPinModeType-pullup"></a>pullup

- <a name="ref-InputPinModeType-pulldown"></a>pulldown

- <a name="ref-InputPinModeType-tristate"></a>tristate

- <a name="ref-InputPinModeType-busholder"></a>busholder



### <a name="ref-TriggerModeType-TriggerModeType"></a>TriggerModeType
- <a name="ref-TriggerModeType-none"></a>none

- <a name="ref-TriggerModeType-rising"></a>rising

- <a name="ref-TriggerModeType-falling"></a>falling

- <a name="ref-TriggerModeType-toggle"></a>toggle

- <a name="ref-TriggerModeType-low"></a>low



### <a name="ref-InterruptPriorityType-InterruptPriorityType"></a>InterruptPriorityType
- <a name="ref-InterruptPriorityType-off"></a>off

- <a name="ref-InterruptPriorityType-low"></a>low

- <a name="ref-InterruptPriorityType-medium"></a>medium

- <a name="ref-InterruptPriorityType-high"></a>high



### <a name="ref-OutputPinModeType-OutputPinModeType"></a>OutputPinModeType
- <a name="ref-OutputPinModeType-wiredOr"></a>wiredOr

- <a name="ref-OutputPinModeType-wiredAnd"></a>wiredAnd

- <a name="ref-OutputPinModeType-wiredOrPull"></a>wiredOrPull

- <a name="ref-OutputPinModeType-wiredAndPull"></a>wiredAndPull



### <a name="ref-InitialStateType-InitialStateType"></a>InitialStateType
- <a name="ref-InitialStateType-low"></a>low

- <a name="ref-InitialStateType-high"></a>high





