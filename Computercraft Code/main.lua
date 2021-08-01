--sleep(10)
dofile('./song.lua')
local tick = 0
local reset = 0
local origins = {{}, {}, {}, {-116, 453}, {-106, 453}, {-106, 463}, {-106, 473}, {-116, 473}, {-115, 462}}
local types = {0, 0, 0, 3, 3, 3, 3, 3, 2}
local beat = 1
local songArray = makeSong()
local mon = peripheral.wrap("monitor_11")
local loop = 1
local loopcounter = 1
local notecount = 0
mon.clear()
mon.setTextScale(2)
mon.setCursorPos(1, 1)
mon.write("Now playing: ")
mon.setCursorPos(1, 2)
mon.write(songArray["title"])
mon.setCursorPos(1, 4)
mon.write("Note count: 0")

function tps(t)
	mon.setCursorPos(1, 3)
	mon.write("Game is running "..(t * 0.2).."x as fast")
	commands.execAsync("tickrate "..(t * 4).." server")
end
function offset(pitch, t)
	z = 5
	p = pitch
	if t == 2 then z = 7 end
	if t == -2 then
		z = 7
		p = p + 24
	end
	if t == 3 then
		z = 9
		p = p + 24
	end
	x = p % z
	y = math.floor(p / z)
	return {y, -x}
end

function offsetgui(pitch)
	x = pitch % 7
	y = math.floor(pitch / 7)
	return {-x + 7, -y}
end

function play(note, pitch)
	if note == -1 then
		tps(pitch / 100)
	else
		notecount = notecount + 1
		if types[note] == 0 then
			commands.execAsync("setblock "..(origins[note][1]).." 4 "..(origins[note][2] + pitch).." redstone_torch")
		else
			block = offset(pitch, types[note])
			commands.execAsync("setblock "..(origins[note][1] + block[1]).." 4 "..(origins[note][2] + block[2]).." redstone_torch")
		end
	end
end
--sleep(10)
tps(songArray["tps"])
while true do
	sleep(0)
	if reset == 0 then
		if tick == 0 then
			worked = pcall(function()
				mon.setCursorPos(1, 5)
				mon.clearLine()
				for i, v in pairs(songArray[beat]) do
					play(v[1], v[2])
					--mon.write("{"..v[1]..","..v[2].."} ")
				end
			end)
			if not worked then
				if loopcounter == loop then
					commands.execAsync("tickrate 20")
					break
				else
					beat = 1
					tick = 0
					reset = 0
					loopcounter = loopcounter + 1
				end
			else
				tick = 1
				reset = 1
				mon.setCursorPos(1, 4)
				mon.clearLine()
				mon.write("Note count: "..notecount)
			end
		else
			commands.execAsync("fill -116 4 473 -98 4 445 glass")
			tick = 0
			reset = 1
			beat = beat + 1
		end
	else
		reset = 0
	end
end