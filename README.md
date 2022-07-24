# Perform
The aim of this project is to allow a band to control thier gig live. 

Perform brings together Mixers, DAW and DMX devices in a show that can be controlled by a band member using a bluetooth foot pedal.

The project aims to be cost effective, the pedal I am using for development cost around £27, the Dmx dongle (RS485) was around £17 and the DMX lights I am using cost around £150 (I had a set of cheap PAR lights and I bought two moving heads second hand).

The DMX communicator started from DMXSimples (https://github.com/BrunoDPO/DMXSimples) 

The OSC code base was based on SharpOSC (https://github.com/ValdemarOrn/SharpOSC)

Thank you to all the open source authors whose code I have drawn in to this.

Additionally the following packages are used:
SharpHook (https://sharphook.tolik.io)

The initial commit allows control of Reaper (DAW) https://www.reaper.fm/ via Open Sound Control (OSC), and allows control of DMX devices connected via a [serial port](https://amzn.eu/d/6eOI6ZH) (either RS232 or RS485). It also listens to the [bluetooth foot controller](https://amzn.eu/d/gZdbdVU) I purchased (set up as a keyboard as the MIDI model didn't seem to work in Windows 10)


## Next steps:
1. Get the show working
2. Implement a basic UI24R client (Okay that wasn't cheap)
3. Add a simple web interface that allows you to:
    - Set default light positions (moving head lights) 
    - View a page that shows the current song, song section and foot pedal actions
4. Design a full show and prove the concept
5. Write some tests (bad me, no!)
6. Add a full UI that will allow the creation and management of all the parts that go to make up a show