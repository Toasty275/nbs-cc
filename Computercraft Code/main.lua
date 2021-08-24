--Manual input variables:
--position of all the notes used ({x1, z1}, {x2, z2}, ... {xn, zn})
local origins = {{-163, 429}, {-163, 430}, {-163, 431}, {-163, 441}, {-155, 433}, {-163, 449}, {-155, 441}, {-161, 433}, {-155, 447}}
--position of where to fill command barriers (x1, z1, x2, z2, y)
local barrier = {-163, 449, -149, 427, 4}
--size of said notes (0 = line, 1 = 5x5, 2 = 7x7, 3 = 9x9, -2 = 7x7 pitch down)
local types = {0, 0, 0, -2, 2, 2, -2, 1, 1}
--monitor of the main display
local mon = peripheral.wrap("monitor_20")
--colors of the notes (used in monitors)
local notecolors = {0, 0, 0, colors.green, colors.lime, colors.yellow, colors.lightBlue, colors.blue, colors.white}
--monitor numbers for each note
local monitors = {0, 0, 0, 21, 22, 25, 23, 24, 26}
--base position of pixel art (x1, z1, x2, z2, y)
local artpos = {-143, 474, -120, 497, 4}

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
	if artstate == nil then artstate = {false, 0} end
	if n == -1 and artstate[1] then
		commands.execAsync("clone "..artpos[1].." "..(artpos[5] + artstate[2]).." "..artpos[2].." "..artpos[3].." "..(artpos[5] + artstate[2]).." "..artpos[4].." -170 4 396")
		artstate[2] = artstate[2] + 1
		if artstate[2] == 4 then artstate[2] = 0 end
	else
		if n == 0 then
			commands.execAsync("clone "..artpos[1].." "..artpos[5].." "..artpos[2].." "..artpos[3].." "..artpos[5].." "..artpos[4].." -170 4 396")
		elseif n == 1 then
			artstate[1] = true
		elseif n == 2 then
			artstate[1] = false
		end
	end
end

function tps(t)
	mon.setCursorPos(1, 3)
	mon.clearLine()
	mon.write("Game is running "..(t * 0.2).."x as fast")
	commands.execAsync("tickrate "..(t * 4).." server")
	tempo = t
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
	elseif note == -2 then
		art(pitch)
	else
		notecount = notecount + 1
		if played[note] == nil then played[note] = {} end
		table.insert(played[note], pitch)
		if types[note] == 0 then
			commands.execAsync("setblock "..(origins[note][1]).." 4 "..(origins[note][2] + pitch).." redstone_torch")
		else
			block = offset(pitch, types[note])
			commands.execAsync("setblock "..(origins[note][1] + block[1]).." 4 "..(origins[note][2] + block[2]).." redstone_torch")
		end
		block = offsetgui(pitch, types[note])
		if not (monitors[note] == 0) then
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
secondstowait = 1000
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
			end)
			if not worked then
				if loopcounter == loop then
					commands.execAsync("tickrate 20")
					--print(err) --uncomment for debugging
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