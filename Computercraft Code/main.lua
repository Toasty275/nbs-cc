--Manual input variables:
--position of all the notes used ({x1, z1}, {x2, z2}, ... {xn, zn})
local origins = {}
origins[0] = {} --harp
origins[1] = {-96, 363} --double bass
origins[2] = {-98, 360} --bass drum
origins[3] = {-99, 362} --snare
origins[4] = {} --click
origins[5] = {-106, 373} --guitar
origins[6] = {-107, 363} --flute
origins[7] = {} --bell
origins[8] = {} --chime
origins[9] = {-96, 373} --xylo
origins[10] = {-96, 355} --iron xylo
origins[11] = {} --cowbell
origins[12] = {} --didgerediedfaiod
origins[13] = {} --bit
origins[14] = {} --banjo
origins[15] = {-106, 355} --pling
--position of where to fill command barriers (x1, z1, x2, z2, y)
local barrier = {-88, 373, -107, 347, 4}
--size of said notes (0 = line, 1 = 5x5, 2 = 7x7, 3 = 9x9, -2 = 7x7 pitch down)
local types = {}
types[0] = 1 --harp
types[1] = 2 --double bass
types[2] = 0 --bass drum
types[3] = 0 --snare
types[4] = 0 --click
types[5] = 3 --guitar
types[6] = -2 --flute
types[7] = 1 --bell
types[8] = 1 --chime
types[9] = 3 --xylo
types[10] = 3 --iron xylo
types[11] = 1 --cowbell
types[12] = 1 --didgerediedfaiod
types[13] = 1 --bit
types[14] = 1 --banjo
types[15] = 3 --pling
--monitor of the main display
local mon = peripheral.wrap("monitor_52")
--colors of the notes (used in monitors)
local notecolors = {}
notecolors[0] = colors.lightBlue --harp
notecolors[1] = colors.green --double bass
notecolors[2] = nil --bass drum
notecolors[3] = nil --snare
notecolors[4] = nil --click
notecolors[5] = colors.yellow --guitar
notecolors[6] = colors.pink --flute
notecolors[7] = colors.magenta --bell
notecolors[8] = colors.purple --chime
notecolors[9] = colors.white --xylo
notecolors[10] = colors.cyan --iron xylo
notecolors[11] = colors.brown --cowbell
notecolors[12] = colors.orange --didgerediedfaiod
notecolors[13] = colors.lime --bit
notecolors[14] = colors.red --banjo
notecolors[15] = colors.blue --pling

--monitor numbers for each note
local monitors = {}
monitors[0] = 0 --harp
monitors[1] = 51 --double bass
monitors[2] = nil --bass drum
monitors[3] = nil --snare
monitors[4] = nil --click
monitors[5] = 47 --guitar
monitors[6] = 46 --flute
monitors[7] = 0 --bell
monitors[8] = 0 --chime
monitors[9] = 53 --xylo
monitors[10] = 50 --iron xylo
monitors[11] = 0 --cowbell
monitors[12] = 0 --didgerediedfaiod
monitors[13] = 0 --bit
monitors[14] = 0 --banjo
monitors[15] = 49 --pling

dofile('./song.lua')
local tick = 0
local reset = 0
local played = {}
local artstate = nil
local beat = 1
local songArray = makeSong()
local loop = 1
local loopcounter = 1
local notecount = 0
local tempo = 0
local currentTempo = 0
mon.clear()
mon.setTextScale(2)
mon.setCursorPos(1, 1)
mon.write("Now playing: ")
mon.setCursorPos(1, 2)
mon.write(songArray["title"])
mon.setCursorPos(1, 4)
mon.write("Note count: 0")

--[[
edit on a per-song basis depending on the pixel art used
n = -1: call every tick
n = 0+: call from the song
--]]
function art(n) 
	if artstate == null then artstate = false end
	if n == 0 then
		commands.execAsync("clone -143 5 344 -136 5 350 -107 7 381")
		artstate = true
	end
	if artstate and n == -1 then
		commands.execAsync("clone -143 4 344 -136 4 350 -107 7 381")
	end
end

function tps(t)
	mon.setCursorPos(1, 3)
	mon.clearLine()
	mon.write("Game is running "..(t * 0.2).."x as fast")
	commands.execAsync("tick rate "..(t * 4))
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
	return {y, -x, z}
end

function offsetgui(pitch, t)
	temp = {}
	temp = offset(pitch, t)
	return {temp[2] + temp[3], -temp[1] + temp[3]}
end
function play(note, pitch)
	if note == -1 then
		tps(pitch / 100)
		tempo = pitch
	elseif note == -2 then
		art(pitch)
	else
		notecount = notecount + 1
		if played[note] == nil then played[note] = {} end
		table.insert(played[note], pitch)
		if types[note] == 0 then
			commands.execAsync("setblock "..(origins[note][1]).." 4 "..(origins[note][2] - pitch).." redstone_torch")
		else
			block = offset(pitch, types[note])
			commands.execAsync("setblock "..(origins[note][1] + block[1]).." 4 "..(origins[note][2] + block[2]).." redstone_torch")
		end
		block = offsetgui(pitch, types[note])
		if not (monitors[note] == nil) then
			w = peripheral.wrap("monitor_"..monitors[note])
			w.setCursorPos(block[1] * 3, block[2] * 2)
			w.setBackgroundColor(notecolors[note])
			w.write("   ")
			w.setCursorPos(block[1] * 3, block[2] * 2 + 1)
			w.write("   ")
		end
	end
end
for i, v in pairs(monitors) do
	if not (v == 0) then
		w = peripheral.wrap("monitor_"..v)
		w.setTextScale(2)
		w.setBackgroundColor(colors.black)
		w.clear()
		w.setBackgroundColor(colors.gray)
		z = 5
		if math.abs(types[i]) == 2 then z = 7
		elseif types[i] == 3 then z = 9
		end
		noteoffset = 0
		if z == 9 then noteoffset = -24
		elseif types[i] == -2 then noteoffset = -24
		end
		for x = 0, z * z, 2 do
			block = offsetgui(x + noteoffset, types[i])
			w.setCursorPos(block[1] * 3, block[2] * 2)
			w.write("   ")
			w.setCursorPos(block[1] * 3, block[2] * 2 + 1)
			w.write("   ")
		end
	end
end
secondstowait = 300
tps(5)
while secondstowait > 0 do
	mon.setCursorPos(1, 5)
	mon.clearLine()
	mon.write("Playing song in ")
	mon.write("0000 seconds")
	mon.setCursorPos(17, 5)
	mon.write(math.floor(secondstowait / 100).."."..(secondstowait - math.floor(secondstowait / 100) * 100))
	secondstowait = secondstowait - 5
	sleep()
end
mon.clearLine()
tps(songArray["tps"])
tempo = songArray["tps"]
currentTempo = tempo
while true do
	sleep(0)
	if reset == 0 then
		if tick == 0 then
			worked, err = pcall(function()
				mon.setCursorPos(1, 5)
				mon.clearLine()
				art(-1)
				for i, v in pairs(songArray[beat]) do
					play(v[1], v[2])
					--mon.write("{"..v[1]..","..v[2].."} ")
				end
				i = 1
				while table.getn(songArray[beat + i]) == 0 do
					i = i + 1
				end
				if not (currentTempo == tempo / i) then
					tps(tempo / i)
					currentTempo = tempo / i
				end
				beat = beat + i - 1
			end)
			if not worked then
				if loopcounter == loop then
					commands.execAsync("tick rate 20")
					print(err) --uncomment for debugging
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
			commands.execAsync("fill "..barrier[1].." "..barrier[5].." "..barrier[2].." "..barrier[3].." "..barrier[5].." "..barrier[4].." barrier")
			for i, v in pairs(monitors) do
				if not (v == 0) then
					w = peripheral.wrap("monitor_"..v)
					if not (played[i] == nil) then
						for ii, vv in pairs(played[i]) do
							if vv % 2 == 0 then
								w.setBackgroundColor(colors.gray)
							else
								w.setBackgroundColor(colors.black)
							end
							block = offsetgui(vv, types[i])
							w.setCursorPos(block[1] * 3, block[2] * 2)
							w.write("   ")
							w.setCursorPos(block[1] * 3, block[2] * 2 + 1)
							w.write("   ")
						end
					end
				end
			end
			played = {}
			tick = 0
			reset = 1
			beat = beat + 1
		end
	else
		reset = 0
	end
end