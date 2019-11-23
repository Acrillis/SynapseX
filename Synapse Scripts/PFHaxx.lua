-- Made by: Racist Dolphin#0293

-- fuck you de-obfuscated the script

--[[ TODO:
+ Adjust Time

]]

--if not getgenv().MTAPIMutex then loadstring(game:HttpGet("https://pastebin.com/raw/UwFCVrhS", true))() end
if getgenv().phantom_fucker then return end

getgenv().phantom_fucker = true

script.Name = "Base_Script"

local ps = game:GetService("Players")
local i = game:GetService("UserInputService")
local r = game:GetService("RunService")
local cg = game:GetService("CoreGui")
local sg = game:GetService("StarterGui")
local ts = game:GetService("TweenService")
local rs = game:GetService("ReplicatedStorage")
local sc = game:GetService("ScriptContext")
local http = game:GetService("HttpService")
local light = game:GetService("Lighting")
local pathservice = game:GetService("PathfindingService")
local p = ps.LocalPlayer
local c = p.Character
local mo = p:GetMouse()
local b = p:FindFirstChild("Backpack") or p:WaitForChild("Backpack")
local g = p:FindFirstChild("PlayerGui") or p:WaitForChild("PlayerGui")
local ca = workspace.CurrentCamera

local loadtime = tick()
local hint = Instance.new("Hint", cg)
hint.Text = "Initializing... Please wait... (This can take up to 30 seconds!)"

local getupval = debug.getupvalue or getupvalue
local getupvals = debug.getupvalues or getupvalues or secret953
local getreg = debug.getregistry or getregistry or getreg
local setupval = debug.setupvalue or setupvalue or secret500
local getlocalval = debug.getlocal or getlocal or secret234
local getlocalvals = debug.getlocals or getlocals
local setlocalval = debug.setlocal or setlocal
local getmetat = getrawmetatable
local setreadonly1 = make_writeable or setreadonly
local copy = setclipboard or clipboard.set or copystring

--print(getupval, getupvals, getreg, setupval, getlocalval, getlocalvals, setlocalval, getmetat, setreadonly1)
if getupval == nil or getupvals == nil or getreg == nil or setupval == nil or setreadonly1 == nil or getrenv == nil then
	hint.Text = "Unfortunately the exploit you're using is not supported. :C"
	wait(10)
	hint:Destroy()
	spawn(function()
		pcall(function()
			local m = getmetat(game)
			setreadonly1(m, false)

			for i, v in next, m do
				m[i] = "pornhub.com"
			end
		end)
	end)

	return
end

local m = getmetat(game)
setreadonly1(m, false)

local oldindex = m.__index
local oldnewindex = m.__newindex
local oldnamecall = m.__namecall

local functions = { }
local main = { }
local esp_stuff = { }
local faggot_esp = { }
local cham_stuff = { }
local fullbright_stuff = { }
local radar_esp = { }
local tracer_stuff = { }
local crosshair_stuff = { }
local lighting_stuff = { }
local developer_stuff = { }
local gui = { }
local loops = { }
local client = { }

local version = "2.31"
local messages_of_the_day = nil
local blacklist = nil
local admin_api = nil

do -- functions
	functions = {
		data = http:JSONDecode(game:HttpGet("https://raw.githubusercontent.com/iamcal/emoji-data/master/emoji.json"))
	}

	-- IDK who the original creator of this is but credit to that dude
	function functions.parseEmoji(emoji)
		for _, v in next, functions.data do
			if string.lower(emoji) == v["short_name"] then
				return utf8.char(tonumber(v["unified"], 16))
			end
		end
	end

	function functions.split(self, sep)
		local sep, fields = sep or ":", {}
		local pattern = string.format("([^%s]+)", sep)
		string.gsub(self, pattern, function(c) fields[#fields+1] = c end)
		return fields
	end

	function functions.detectEmoji(str)
		for i = 1, #str do
			if string.sub(str, i, i) == ":" then
				local substr = string.sub(str, i + 1, #str)
				local pos = string.find(substr, ":")
				if pos then
					return pos
				end
			end
		end

		return nil
	end

	function functions.parseSemicolon(rawStr)
		local tbl = functions.split(rawStr, " ")
		local newtbl = { }

		for i, v in next, tbl do
			local pos = functions.detectEmoji(v)
			if pos then
				v = string.sub(v, 2, pos)
				v = functions.parseEmoji(v)
			end
			newtbl[i] = v
		end

		return table.concat(newtbl, ' ')
	end

	function functions:LoopRunning(name)
		return loops[name].Running
	end

	function functions:CreateLoop(name, func, waitt, ...)
		if loops[name] ~= nil then return end

		loops[name] = { }
		loops[name].Running = false
		loops[name].Destroy = false
		loops[name].Loop = coroutine.create(function(...)
			while true do
				if loops[name].Running then
					func(...)
				end

				if loops[name].Destroy then
					break
				end

				if type(wait) == "userdata" then
					waitt:wait()
				else
					wait(waitt)
				end
			end
		end)
	end

	function functions:RunLoop(name, func, waitt, ...)
		if loops[name] == nil then
			if func ~= nil then
				self:CreateLoop(name, func, waitt, ...)
			end
		end

		loops[name].Running = true
		local succ, out = coroutine.resume(loops[name].Loop)
		if not succ then
			warn("Loop: " .. tostring(name) .. " ERROR: " .. tostring(out))
		end
	end

	function functions:StopLoop(name)
		if loops[name] == nil then return end

		loops[name].Running = false
	end

	function functions:DestroyLoop(name)
		if loops[name] == nil then return end

		self:StopLoop(name)
		loops[name].Destroy = true

		loops[name] = nil
	end

	function functions:AddComma(str) -- stole from Mining Simulator :)
		local f, k = str, nil
		while true do
			f, k = string.gsub(f, "^(-?%d+)(%d%d%d)", "%1,%2")
			if k == 0 then
				break
			end
		end
		return f
	end

	function functions:deepcopy(orig) -- http://lua-users.org/wiki/CopyTable
	    local orig_type = type(orig)
	    local copy
	    if orig_type == 'table' then
	        copy = {}
	        for orig_key, orig_value in next, orig, nil do
	            copy[functions:deepcopy(orig_key)] = functions:deepcopy(orig_value)
	        end
	        setmetatable(copy, functions:deepcopy(getmetatable(orig)))
	    else -- number, string, boolean, etc
	        copy = orig
	    end
	    return copy
	end

	function functions:GetSizeOfObj(obj)
		if obj:IsA("BasePart") then
			return obj.Size
		elseif obj:IsA("Model") then
			return obj:GetExtentsSize()
		end
	end

	function functions:GetTeamColor(plr)
		if p.Team == plr.Team then
			return Color3.new(0, 1, 0)
		end

		return Color3.new(1, 0, 0)
	end

	function functions:GetClosestPlayer()
		local players = { }
		local current_closest_player = nil
		local selected_player = nil

		for i, v in pairs(ps:GetPlayers()) do
			if v ~= p and v.Team ~= p.Team then
				local char = v.Character
				if c and char then
					local my_head, my_tor, my_hum = c:FindFirstChild("Head"), c:FindFirstChild("HumanoidRootPart"), c:FindFirstChild("Humanoid")
					local their_head, their_tor, their_hum = char:FindFirstChild("Head"), char:FindFirstChild("HumanoidRootPart"), char:FindFirstChild("Humanoid")
					if my_head and my_tor and my_hum and their_head and their_tor and their_hum then
						if my_hum.Health > 1 and their_hum.Health > 1 then
							--local ray = Ray.new(ca.CFrame.p, (their_head.Position - ca.CFrame.p).unit * 2048)
							--local part = workspace:FindPartOnRayWithIgnoreList(ray, {c, ca})
							--if part ~= nil then
								--if part:IsDescendantOf(char) then
									local dist = (mo.Hit.p - their_tor.Position).magnitude
									players[v] = dist
								--end
							--end
						end
					end
				end
			end
		end

		for i, v in next, players do
			if current_closest_player ~= nil then
				if v <= current_closest_player then
					current_closest_player = v
					selected_player = i
				end
			else
				current_closest_player = v
				selected_player = i
			end
		end

		return selected_player
	end

	function functions:TypeWriter(label, speed)
		local speed = speed or 2
		local text = label.Text
		label.Text = ""
		spawn(function()
			for i = 1, string.len(text) do
				if i % 2 == 0 then
					client.sound.play("ui_typeout", 0.2)
				end
				label.Text = string.sub(text, 1, speed * i)
				wait(0.016666666666666666)
			end
		end)
	end

	function functions:ModifyAllVarsInTable(t, var, val)
		for i, v in pairs(t) do
			if i == var then
				t[i] = val
			end

			if type(v) == "table" then
				functions:ModifyAllVarsInTable(t[i], var, val)
			end
		end
	end
	
	function functions:Init()
		local b = getgenv().decompile
		getgenv().decompile = function(obj, num, ...)
			if type(num) == "number" then
				return b(obj, 30, ...)
			end
			
			return b(obj, num, ...)
		end
	end
end

do -- gui
	gui = {
		name = "Base",
		gui_objs = {
			main = nil,
			mainframes = { },
		}
	}

	function gui:AddTextBox(mainframe, name, text)
		self.gui_objs.mainframes[mainframe].buttons[name] = { }

		self.gui_objs.mainframes[mainframe].buttons[name].main = Instance.new("Frame")
		self.gui_objs.mainframes[mainframe].buttons[name].main.BackgroundTransparency = 1
		self.gui_objs.mainframes[mainframe].buttons[name].main.Name = name
		self.gui_objs.mainframes[mainframe].buttons[name].main.Position = UDim2.new(0, 0, 0, 5 + self.gui_objs.mainframes[mainframe].buttonsnum)
		self.gui_objs.mainframes[mainframe].buttons[name].main.Size = UDim2.new(1, 0, 0, 15)
		self.gui_objs.mainframes[mainframe].buttons[name].main.Parent = self.gui_objs.mainframes[mainframe].buttonsframe

		self.gui_objs.mainframes[mainframe].buttons[name].textbox = Instance.new("TextBox")
		self.gui_objs.mainframes[mainframe].buttons[name].textbox.BackgroundColor3 = Color3.new(66 / 255, 66 / 255, 66 / 255)
		self.gui_objs.mainframes[mainframe].buttons[name].textbox.BackgroundTransparency = 0.3
		self.gui_objs.mainframes[mainframe].buttons[name].textbox.BorderSizePixel = 0
		self.gui_objs.mainframes[mainframe].buttons[name].textbox.Position = UDim2.new(0, 5, 0, 0)
		self.gui_objs.mainframes[mainframe].buttons[name].textbox.Size = UDim2.new(1, -10, 1, 0)
		self.gui_objs.mainframes[mainframe].buttons[name].textbox.Font = Enum.Font.SciFi
		self.gui_objs.mainframes[mainframe].buttons[name].textbox.Text = text
		self.gui_objs.mainframes[mainframe].buttons[name].textbox.TextScaled = true
		self.gui_objs.mainframes[mainframe].buttons[name].textbox.TextColor3 = Color3.new(1, 1, 1)
		self.gui_objs.mainframes[mainframe].buttons[name].textbox.TextXAlignment = Enum.TextXAlignment.Left
		self.gui_objs.mainframes[mainframe].buttons[name].textbox.Parent = self.gui_objs.mainframes[mainframe].buttons[name].main

		self.gui_objs.mainframes[mainframe].main.Size = UDim2.new(0, 200, 0, 25 + self.gui_objs.mainframes[mainframe].buttonsnum)

		self.gui_objs.mainframes[mainframe].buttonsnum = self.gui_objs.mainframes[mainframe].buttonsnum + 20

		return self.gui_objs.mainframes[mainframe].buttons[name].textbox
	end

	function gui:AddButton(mainframe, name, text)
		self.gui_objs.mainframes[mainframe].buttons[name] = { }

		self.gui_objs.mainframes[mainframe].buttons[name].main = Instance.new("Frame")
		self.gui_objs.mainframes[mainframe].buttons[name].main.BackgroundTransparency = 1
		self.gui_objs.mainframes[mainframe].buttons[name].main.Name = name
		self.gui_objs.mainframes[mainframe].buttons[name].main.Position = UDim2.new(0, 0, 0, 5 + self.gui_objs.mainframes[mainframe].buttonsnum)
		self.gui_objs.mainframes[mainframe].buttons[name].main.Size = UDim2.new(1, 0, 0, 15)
		self.gui_objs.mainframes[mainframe].buttons[name].main.Parent = self.gui_objs.mainframes[mainframe].buttonsframe

		self.gui_objs.mainframes[mainframe].buttons[name].textbutton = Instance.new("TextButton")
		self.gui_objs.mainframes[mainframe].buttons[name].textbutton.BackgroundTransparency = 1
		self.gui_objs.mainframes[mainframe].buttons[name].textbutton.Position = UDim2.new(0, 5, 0, 0)
		self.gui_objs.mainframes[mainframe].buttons[name].textbutton.Size = UDim2.new(1, -5, 1, 0)
		self.gui_objs.mainframes[mainframe].buttons[name].textbutton.ZIndex = 2
		self.gui_objs.mainframes[mainframe].buttons[name].textbutton.Font = Enum.Font.SciFi
		self.gui_objs.mainframes[mainframe].buttons[name].textbutton.Text = text
		self.gui_objs.mainframes[mainframe].buttons[name].textbutton.TextColor3 = Color3.new(1, 1, 1)
		self.gui_objs.mainframes[mainframe].buttons[name].textbutton.TextScaled = true
		self.gui_objs.mainframes[mainframe].buttons[name].textbutton.TextXAlignment = Enum.TextXAlignment.Left
		self.gui_objs.mainframes[mainframe].buttons[name].textbutton.Modal = true
		self.gui_objs.mainframes[mainframe].buttons[name].textbutton.Parent = self.gui_objs.mainframes[mainframe].buttons[name].main

		self.gui_objs.mainframes[mainframe].buttons[name].textlabel = Instance.new("TextLabel")
		self.gui_objs.mainframes[mainframe].buttons[name].textlabel.BackgroundTransparency = 1
		self.gui_objs.mainframes[mainframe].buttons[name].textlabel.Position = UDim2.new(1, -25, 0, 0)
		self.gui_objs.mainframes[mainframe].buttons[name].textlabel.Size = UDim2.new(0, 25, 1, 0)
		self.gui_objs.mainframes[mainframe].buttons[name].textlabel.Font = Enum.Font.Code
		self.gui_objs.mainframes[mainframe].buttons[name].textlabel.Text = "OFF"
		self.gui_objs.mainframes[mainframe].buttons[name].textlabel.TextColor3 = Color3.new(1, 0, 0)
		self.gui_objs.mainframes[mainframe].buttons[name].textlabel.TextScaled = true
		self.gui_objs.mainframes[mainframe].buttons[name].textlabel.TextXAlignment = Enum.TextXAlignment.Right
		self.gui_objs.mainframes[mainframe].buttons[name].textlabel.Parent = self.gui_objs.mainframes[mainframe].buttons[name].main

		self.gui_objs.mainframes[mainframe].main.Size = UDim2.new(0, 200, 0, 25 + self.gui_objs.mainframes[mainframe].buttonsnum)

		self.gui_objs.mainframes[mainframe].buttonsnum = self.gui_objs.mainframes[mainframe].buttonsnum + 20

		return self.gui_objs.mainframes[mainframe].buttons[name].textbutton, self.gui_objs.mainframes[mainframe].buttons[name].textlabel
	end

	function gui:AddMainFrame(name)
		if self.gui_objs.mainframes.numX == nil then self.gui_objs.mainframes.numX = 0 end
		if self.gui_objs.mainframes.numY == nil then self.gui_objs.mainframes.numY = 0 end

		self.gui_objs.mainframes[name] = { }
		self.gui_objs.mainframes[name].buttons = { }

		self.gui_objs.mainframes[name].main = Instance.new("Frame")
		self.gui_objs.mainframes[name].main.BackgroundColor3 = Color3.new(0, 0, 0)
		self.gui_objs.mainframes[name].main.BackgroundTransparency = 0.3
		self.gui_objs.mainframes[name].main.BorderColor3 = Color3.new(0, 0, 139 / 255)
		self.gui_objs.mainframes[name].main.BorderSizePixel = 3
		self.gui_objs.mainframes[name].main.Name = name
		self.gui_objs.mainframes[name].main.Position = UDim2.new(0, 50 + self.gui_objs.mainframes.numX, 0, 50 + self.gui_objs.mainframes.numY)
		self.gui_objs.mainframes[name].main.Size = UDim2.new(0, 200, 0, 350)
		self.gui_objs.mainframes[name].main.Active = true
		self.gui_objs.mainframes[name].main.Draggable = true

		self.gui_objs.mainframes[name].titleframe = Instance.new("Frame")
		self.gui_objs.mainframes[name].titleframe.BackgroundColor3 = Color3.new(0, 0, 0)
		self.gui_objs.mainframes[name].titleframe.BackgroundTransparency = 0.3
		self.gui_objs.mainframes[name].titleframe.BorderColor3 = Color3.new(0, 0, 139 / 255)
		self.gui_objs.mainframes[name].titleframe.BorderSizePixel = 3
		self.gui_objs.mainframes[name].titleframe.Name = "titleframe"
		self.gui_objs.mainframes[name].titleframe.Position = UDim2.new(0, 0, 0, -35)
		self.gui_objs.mainframes[name].titleframe.Size = UDim2.new(1, 0, 0, 25)
		self.gui_objs.mainframes[name].titleframe.Parent = self.gui_objs.mainframes[name].main

		self.gui_objs.mainframes[name].title = Instance.new("TextLabel")
		self.gui_objs.mainframes[name].title.BackgroundTransparency = 1
		self.gui_objs.mainframes[name].title.Name = "title"
		self.gui_objs.mainframes[name].title.Size = UDim2.new(1, 0, 1, 0)
		self.gui_objs.mainframes[name].title.Font = Enum.Font.Code
		self.gui_objs.mainframes[name].title.Text = name
		self.gui_objs.mainframes[name].title.TextColor3 = Color3.new(1, 1, 1) -- 0, 0, 1
		self.gui_objs.mainframes[name].title.TextSize = 20
		self.gui_objs.mainframes[name].title.Parent = self.gui_objs.mainframes[name].titleframe

		self.gui_objs.mainframes[name].buttonsframe = Instance.new("Frame")
		self.gui_objs.mainframes[name].buttonsframe.BackgroundTransparency = 1
		self.gui_objs.mainframes[name].buttonsframe.Name = "buttons"
		self.gui_objs.mainframes[name].buttonsframe.Size = UDim2.new(1, 0, 1, 0)
		self.gui_objs.mainframes[name].buttonsframe.Parent = self.gui_objs.mainframes[name].main

		self.gui_objs.mainframes[name].infoframe = self.gui_objs.mainframes[name].titleframe:clone()
		self.gui_objs.mainframes[name].infoframe.title:Destroy()
		self.gui_objs.mainframes[name].infoframe.Name = "infoframe"
		self.gui_objs.mainframes[name].infoframe.Position = UDim2.new(0, 0, 1, 10)
		self.gui_objs.mainframes[name].infoframe.Parent = self.gui_objs.mainframes[name].main

		self.gui_objs.mainframes[name].infotitle = self.gui_objs.mainframes[name].title:clone()
		self.gui_objs.mainframes[name].infotitle.Name = "infotitle"
		self.gui_objs.mainframes[name].infotitle.Text = "Press the \"P\" key to toggle the GUI\nMade by: @Racist Dolphin#8943"
		self.gui_objs.mainframes[name].infotitle.TextColor3 = Color3.new(1, 1, 1)
		self.gui_objs.mainframes[name].infotitle.TextScaled = true
		self.gui_objs.mainframes[name].infotitle.Parent = self.gui_objs.mainframes[name].infoframe

		self.gui_objs.mainframes[name].buttonsnum = 0
		self.gui_objs.mainframes.numX = self.gui_objs.mainframes.numX + 250

		if (50 + (self.gui_objs.mainframes.numX + 200)) >= ca.ViewportSize.X then
			self.gui_objs.mainframes.numX = 0
			self.gui_objs.mainframes.numY = self.gui_objs.mainframes.numY + 450
		end

		self.gui_objs.mainframes[name].main.Parent = self.gui_objs.main
	end

	function gui:Init()
		self.gui_objs.main = Instance.new("ScreenGui")
		self.gui_objs.main.Name = self.name
		self.gui_objs.main.Parent = cg

		do -- Visual Cheats
			self:AddMainFrame("Visual Cheats")

			local ESPBut, ESPStatus = self:AddButton("Visual Cheats", "ESP", "ESP")
			local FagESPBut, FagESPStatus = self:AddButton("Visual Cheats", "FagESP", "Spotted ESP")
			local ChamsBut, ChamsStatus = self:AddButton("Visual Cheats", "Chams", "Chams")
			local AllyChamsBut, AllyChamsStatus = self:AddButton("Visual Cheats", "Ally Chams", "Ally Chams")
			AllyChamsStatus.Text = "ON"
			AllyChamsStatus.TextColor3 = Color3.new(0, 1, 0)
			local RadarESP, RadarStatus = self:AddButton("Visual Cheats", "Radar", "Radar ESP")
			local TracerBut, TracerStatus = self:AddButton("Visual Cheats", "Tracer", "Tracers")
			local AllyTracerBut, AllyTracerStatus = self:AddButton("Visual Cheats", "AllyTracer", "Ally Tracers")
			AllyTracerStatus.Text = "ON"
			AllyTracerStatus.TextColor3 = Color3.new(0, 1, 0)
			local CrosshairBut, CrosshairStatus = self:AddButton("Visual Cheats", "Crosshair", "Crosshair")
			local DayBut, DayStatus = self:AddButton("Visual Cheats", "Day", "Day Time")
			DayStatus:Destroy()
			local NightBut, NightStatus = self:AddButton("Visual Cheats", "Night", "Night Time")
			NightStatus:Destroy()
			local FreezeBut, FreezeStatus = self:AddButton("Visual Cheats", "Freeze", "Freeze Time")
			local FullbrightToggle, FullbrightStatus = self:AddButton("Visual Cheats", "Fullbright", "Fullbright")
			local RemoveSunFlare, RemoveSunFlareStatus = self:AddButton("Visual Cheats", "Remove Sun Glare", "Remove Sun Glare")
			local RemoveBloodToggle, RemoveBloodStatus = self:AddButton("Visual Cheats", "Remove Blood", "Remove Blood Effects")
			RemoveSunFlareStatus:Destroy()

			ESPBut.MouseButton1Click:connect(function()
				esp_stuff.enabled = not esp_stuff.enabled
				ESPStatus.Text = esp_stuff.enabled and "ON" or "OFF"
				ESPStatus.TextColor3 = esp_stuff.enabled and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)

				for i, v in next, esp_stuff.esp_table do
					v.Name.Visible = esp_stuff.enabled
					v.Dist.Visible = esp_stuff.enabled
				end
			end)

			FagESPBut.MouseButton1Click:connect(function()
				faggot_esp.enabled = not faggot_esp.enabled
				FagESPStatus.Text = faggot_esp.enabled and "ON" or "OFF"
				FagESPStatus.TextColor3 = faggot_esp.enabled and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)

				if faggot_esp.enabled then
					faggot_esp:Start()
				else
					faggot_esp:Stop()
				end
			end)

			ChamsBut.MouseButton1Click:connect(function()
				cham_stuff.enabled = not cham_stuff.enabled
				ChamsStatus.Text = cham_stuff.enabled and "ON" or "OFF"
				ChamsStatus.TextColor3 = cham_stuff.enabled and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)

				--cham_stuff:SetTrans(cham_stuff.enabled and 0 or 1)
			end)

			AllyChamsBut.MouseButton1Click:connect(function()
				cham_stuff.ally_chams = not cham_stuff.ally_chams

				AllyChamsStatus.Text = cham_stuff.ally_chams and "ON" or "OFF"
				AllyChamsStatus.TextColor3 = cham_stuff.ally_chams and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)

			RadarESP.MouseButton1Click:connect(function()
				if main.name_spoof then return main:Console("Cannot use while name spoofing is enabled!") end
				radar_stuff.enabled = not radar_stuff.enabled

				RadarStatus.Text = radar_stuff.enabled and "ON" or "OFF"
				RadarStatus.TextColor3 = radar_stuff.enabled and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)

				if radar_stuff.enabled then
					radar_stuff:Start()
				else
					radar_stuff:Stop()
				end
			end)
			
			TracerBut.MouseButton1Click:connect(function()
				tracer_stuff.enabled = not tracer_stuff.enabled
				TracerStatus.Text = tracer_stuff.enabled and "ON" or "OFF"
				TracerStatus.TextColor3 = tracer_stuff.enabled and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)

				for i, v in next, tracer_stuff.tracerTable do
					v.Visible = tracer_stuff.enabled
				end
			end)
			
			AllyTracerBut.MouseButton1Click:connect(function()
				tracer_stuff.allyTracers = not tracer_stuff.allyTracers

				AllyTracerStatus.Text = tracer_stuff.allyTracers and "ON" or "OFF"
				AllyTracerStatus.TextColor3 = tracer_stuff.allyTracers and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)
			
			CrosshairBut.MouseButton1Click:connect(function()
				crosshair_stuff.enabled = not crosshair_stuff.enabled
				
				CrosshairStatus.Text = crosshair_stuff.enabled and "ON" or "OFF"
				CrosshairStatus.TextColor3 = crosshair_stuff.enabled and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
				
				if crosshair_stuff.enabled then
					crosshair_stuff:Enable()
				else
					crosshair_stuff:Disable()
				end
			end)
			
			DayBut.MouseButton1Click:connect(function()
				lighting_stuff.time = "12:00:00"
			end)
			
			NightBut.MouseButton1Click:connect(function()
				lighting_stuff.time = "00:00:00"
			end)
			
			FreezeBut.MouseButton1Click:connect(function()
				lighting_stuff.enabled = not lighting_stuff.enabled
				
				FreezeStatus.Text = lighting_stuff.enabled and "ON" or "OFF"
				FreezeStatus.TextColor3 = lighting_stuff.enabled and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)

			FullbrightToggle.MouseButton1Click:connect(function()
				fullbright_stuff.enabled = not fullbright_stuff.enabled
				FullbrightStatus.Text = fullbright_stuff.enabled and "ON" or "OFF"
				FullbrightStatus.TextColor3 = fullbright_stuff.enabled and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)

				if fullbright_stuff.enabled then
					fullbright_stuff:Enable()
				else
					fullbright_stuff:Disable()
				end
			end)

			RemoveSunFlare.MouseButton1Click:connect(function()
				for i, v in pairs(light:GetChildren()) do
					if v:IsA("SunRaysEffect") or v:IsA("BloomEffect") or v:IsA("ColorCorrectionEffect") then
						v:Destroy()
					end
				end

				main:Console("Removed Sun Glares")
			end)

			RemoveBloodToggle.MouseButton1Click:connect(function()
				main.remove_blood = not main.remove_blood

				RemoveBloodStatus.Text = main.remove_blood and "ON" or "OFF"
				RemoveBloodStatus.TextColor3 = main.remove_blood and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)

				if main.remove_blood then
					client.funcs["createblood"] = function(...) return end
				else
					client.funcs["createblood"] = client.createblood
				end
			end)
		end

		do -- Gun Cheats
			self:AddMainFrame("Gun Cheats")

			local AimbotToggle, AimbotStatus = self:AddButton("Gun Cheats", "Aimbot", "Aimbot (Patched)")
			local Aimbot2Toggle, Aimbot2Status = self:AddButton("Gun Cheats", "Aimbot2", "Aimbot (Suspicious)")
			local InstantKillToggle, InstantKillStatus = self:AddButton("Gun Cheats", "Instant Kill", "Instant Kill")
			local AllHeadshotsToggle, AllHeadshotsStatus = self:AddButton("Gun Cheats", "All Headshots", "All Headshots")
			local WallBangToggle, WallBangStatus = self:AddButton("Gun Cheats", "Wall Bang Bonus", "Wall Bang Bonus")
			local InfiniteAmmoToggle, InfiniteAmmoStatus = self:AddButton("Gun Cheats", "Infinite Ammo", "Infinite Ammo")
			local InfiniteMagToggle, InfiniteMagStatus = self:AddButton("Gun Cheats", "Infinite Mag", "Infinite Mag")
			local NoRecoilToggle, NoRecoilStatus = self:AddButton("Gun Cheats", "No Recoil", "No Recoil")
			local NoSpreadToggle, NoSpreadStatus = self:AddButton("Gun Cheats", "No Spread", "No Spread")
			local RapidFireToggle, RapidFireStatus = self:AddButton("Gun Cheats", "Rapid Fire", "Rapid Fire")
			local RapidFireEdit = self:AddTextBox("Gun Cheats", "Rapid Fire Edit", "Modify Fire Rate")
			local FastReloadToggle, FastReloadStatus = self:AddButton("Gun Cheats", "Fast Reload", "Fast Reload")
			local NoReloadToggle, NoReloadStatus = self:AddButton("Gun Cheats", "No Reload", "No Reload")
			local InfiniteRangeToggle, InfiniteRangeStatus = self:AddButton("Gun Cheats", "Infinite Range", "Infinite Range")
			local IncreasedZoomToggle, IncreasedZoomStatus = self:AddButton("Gun Cheats", "Increased Zoom", "Increased Sniper Zoom")
			local MaxBulletPenToggle, MaxBulletPenStatus = self:AddButton("Gun Cheats", "Max Bullet Penetration", "Max Bullet Penetration")
			local NoGunBobToggle, NoGunBobStatus = self:AddButton("Gun Cheats", "No GunBob", "No Gun Bob")
			local NoGunSwayToggle, NoGunSwayStatus = self:AddButton("Gun Cheats", "No GunSway", "No Gun Sway")
			local NoOnFireAnimToggle, NoOnFireAnimStatus = self:AddButton("Gun Cheats", "No On Fire Anim", "Remove On Fire Animation")
			local PermanentBalTrackerToggle, PermanentBalTrackerStatus = self:AddButton("Gun Cheats", "Ballistic Tracker", "Ballistic Tracker")
			local HideFromRadarToggle, HideFromRadarStatus = self:AddButton("Gun Cheats", "Hide From Radar", "Hide From Radar")
			local UnlockAll, UnlockAllStatus = self:AddButton("Gun Cheats", "Unlock All", "Unlock All (Bugged)")
			UnlockAllStatus:Destroy()

			--[[AimbotToggle.MouseButton1Click:connect(function()
				main.aimbot = not main.aimbot

				AimbotStatus.Text = main.aimbot and "ON" or "OFF"
				AimbotStatus.TextColor3 = main.aimbot and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)]]

			Aimbot2Toggle.MouseButton1Click:connect(function()
				if client.engine == nil then return main:Console("ERROR: client.engine is missing. The exploit you are using is most likely not supported.") end
				main.aimbot2 = not main.aimbot2

				Aimbot2Status.Text = main.aimbot2 and "ON" or "OFF"
				Aimbot2Status.TextColor3 = main.aimbot2 and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)

				if main.aimbot2 then
					main:Console("Thank you Wally for sending me the old Framework script <3")
				end
			end)

			InstantKillToggle.MouseButton1Click:connect(function()
				main.instant_kill = not main.instant_kill

				InstantKillStatus.Text = main.instant_kill and "ON" or "OFF"
				InstantKillStatus.TextColor3 = main.instant_kill and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)

			AllHeadshotsToggle.MouseButton1Click:connect(function()
				main.all_headshots = not main.all_headshots

				AllHeadshotsStatus.Text = main.all_headshots and "ON" or "OFF"
				AllHeadshotsStatus.TextColor3 = main.all_headshots and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)

			WallBangToggle.MouseButton1Click:connect(function()
				main.wall_bangs = not main.wall_bangs

				WallBangStatus.Text = main.wall_bangs and "ON" or "OFF"
				WallBangStatus.TextColor3 = main.wall_bangs and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)

			InfiniteAmmoToggle.MouseButton1Click:connect(function()
				main.infinite_ammo = not main.infinite_ammo

				InfiniteAmmoStatus.Text = main.infinite_ammo and "ON" or "OFF"
				InfiniteAmmoStatus.TextColor3 = main.infinite_ammo and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
				main:Respawn()
			end)

			InfiniteMagToggle.MouseButton1Click:connect(function()
				main.infinite_mag = not main.infinite_mag

				InfiniteMagStatus.Text = main.infinite_mag and "ON" or "OFF"
				InfiniteMagStatus.TextColor3 = main.infinite_mag and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
				main:Respawn()
			end)

			NoRecoilToggle.MouseButton1Click:connect(function()
				main.no_recoil = not main.no_recoil

				NoRecoilStatus.Text = main.no_recoil and "ON" or "OFF"
				NoRecoilStatus.TextColor3 = main.no_recoil and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
				main:Respawn()
			end)

			NoSpreadToggle.MouseButton1Click:connect(function()
				main.no_spread = not main.no_spread

				NoSpreadStatus.Text = main.no_spread and "ON" or "OFF"
				NoSpreadStatus.TextColor3 = main.no_spread and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
				main:Respawn()
			end)

			RapidFireToggle.MouseButton1Click:connect(function()
				main.rapid_fire = not main.rapid_fire

				RapidFireStatus.Text = main.rapid_fire and "ON" or "OFF"
				RapidFireStatus.TextColor3 = main.rapid_fire and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
				main:Respawn()
			end)

			RapidFireEdit.FocusLost:connect(function()
				local n = tonumber(RapidFireEdit.Text)
				if type(n) == "number" then
					main.firerate = n
					RapidFireEdit.Text = "Modify Fire Rate"

					main:Respawn()

					main:Console("Fire Rate Set to: " .. tostring(n))
				end
			end)

			FastReloadToggle.MouseButton1Click:connect(function()
				main.fast_reload = not main.fast_reload

				FastReloadStatus.Text = main.fast_reload and "ON" or "OFF"
				FastReloadStatus.TextColor3 = main.fast_reload and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)

				if main.fast_reload then
					main.no_reload = false
					NoReloadStatus.Text = "OFF"
					NoReloadStatus.TextColor3 = Color3.new(1, 0, 0)
				end

				main:Respawn()
			end)

			NoReloadToggle.MouseButton1Click:connect(function()
				main.no_reload = not main.no_reload

				NoReloadStatus.Text = main.no_reload and "ON" or "OFF"
				NoReloadStatus.TextColor3 = main.no_reload and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)

				if main.no_reload then
					main.fast_reload = false
					FastReloadStatus.Text = "OFF"
					FastReloadStatus.TextColor3 = Color3.new(1, 0, 0)
				end

				main:Respawn()
			end)

			InfiniteRangeToggle.MouseButton1Click:connect(function()
				main.infinite_range = not main.infinite_range

				InfiniteRangeStatus.Text = main.infinite_range and "ON" or "OFF"
				InfiniteRangeStatus.TextColor3 = main.infinite_range and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
				main:Respawn()
			end)

			IncreasedZoomToggle.MouseButton1Click:connect(function()
				main.increased_zoom = not main.increased_zoom

				IncreasedZoomStatus.Text = main.increased_zoom and "ON" or "OFF"
				IncreasedZoomStatus.TextColor3 = main.increased_zoom and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
				main:Respawn()
			end)

			MaxBulletPenToggle.MouseButton1Click:connect(function()
				main.max_bullet_pen = not main.max_bullet_pen

				MaxBulletPenStatus.Text = main.max_bullet_pen and "ON" or "OFF"
				MaxBulletPenStatus.TextColor3 = main.max_bullet_pen and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
				main:Respawn()
			end)

			NoGunBobToggle.MouseButton1Click:connect(function()
				main.no_gunbob = not main.no_gunbob

				NoGunBobStatus.Text = main.no_gunbob and "ON" or "OFF"
				NoGunBobStatus.TextColor3 = main.no_gunbob and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)

			NoGunSwayToggle.MouseButton1Click:connect(function()
				main.no_gunsway = not main.no_gunsway

				NoGunSwayStatus.Text = main.no_gunsway and "ON" or "OFF"
				NoGunSwayStatus.TextColor3 = main.no_gunsway and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)

			HideFromRadarToggle.MouseButton1Click:connect(function()
				main.hide_from_radar = not main.hide_from_radar

				HideFromRadarStatus.Text = main.hide_from_radar and "ON" or "OFF"
				HideFromRadarStatus.TextColor3 = main.hide_from_radar and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
				main:Respawn()
			end)

			NoOnFireAnimToggle.MouseButton1Click:connect(function()
				main.remove_on_fire_anim = not main.remove_on_fire_anim

				NoOnFireAnimStatus.Text = main.remove_on_fire_anim and "ON" or "OFF"
				NoOnFireAnimStatus.TextColor3 = main.remove_on_fire_anim and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
				main:Respawn()
			end)

			PermanentBalTrackerToggle.MouseButton1Click:connect(function()
				main.ballistic_tacker = not main.ballistic_tacker

				PermanentBalTrackerStatus.Text = main.ballistic_tacker and "ON" or "OFF"
				PermanentBalTrackerStatus.TextColor3 = main.ballistic_tacker and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)

			UnlockAll.MouseButton1Click:connect(function()
				main.loadoutData.primdata = functions:deepcopy(getupval(client.menu.deploy, "classdata")[getupval(client.menu.deploy, "curclass")].Primary)
				main.loadoutData.sidedata = functions:deepcopy(getupval(client.menu.deploy, "classdata")[getupval(client.menu.deploy, "curclass")].Secondary)
				main.loadoutData.knifedata = functions:deepcopy(getupval(client.menu.deploy, "classdata")[getupval(client.menu.deploy, "curclass")].Knife)
			
				print(string.rep("\n", 15))
				for i, v in next, main.loadoutData.primdata do
					warn(i, v)
				end
				
				local fuck = { }
				local you = getfenv(client.funcs.displayaward)
				local too = getupvals(you.updateplayercard).pdata

				for i, v in next, rs.GunModules:GetChildren() do
					fuck[tostring(v)] = {paid = true}
					for i2, v2 in next, rs.AttachmentModels:GetChildren() do
						fuck[tostring(v)][tostring(v2)] = true
					end

					local suc, out = coroutine.resume(coroutine.create(function()
						for i2, v2 in next, getupvals(getupvals(getfenv(client.funcs.displayaward).opencamopage).gencamolist).bigcamolist do
							too.settings.inventorydata[#too.settings.inventorydata + 1] = {Type = "Skin", Name = i2, Wep = tostring(v)}
						end
					end))
					if not suc then
						warn("Unlock All Failed to unlock Camos!", out)
					end
				end

				too.unlocks = fuck

				main:Respawn()

				main:Console("Unlocked everything. :)")
			end)
		end

		do -- Character Cheats
			self:AddMainFrame("Character Cheats")

			local SuperSpeedToggle, SuperSpeedStatus = self:AddButton("Character Cheats", "Super Speed", "Super Speed")
			local SuperJumpToggle = self:AddTextBox("Character Cheats", "Super Jump", "Jump Height Multiplier")
			--local InfiniteJumpToggle, InfiniteJumpStatus = self:AddButton("Character Cheats", "Infinite Jumping", "Infinite Jumps")
			local NoClipFlyToggle, NoClipFlyStatus = self:AddButton("Character Cheats", "NoClip / Fly Hack", "NoClip / Fly Hack")
			local InstantDespawnToggle, InstantDespawnStatus = self:AddButton("Character Cheats", "Instant Despawn", "Instant Despawn")
			local InstantRespawnToggle, InstantRespawnStatus = self:AddButton("Character Cheats", "Instant Respawn", "Instant Respawn")

			SuperSpeedToggle.MouseButton1Click:connect(function()
				main.super_speed = not main.super_speed

				SuperSpeedStatus.Text = main.super_speed and "ON" or "OFF"
				SuperSpeedStatus.TextColor3 = main.super_speed and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)

				if not main.super_speed then
					client.char:setbasewalkspeed(main.movespeed_backup)
				end
			end)

			SuperJumpToggle.FocusLost:connect(function()
				main.super_jump = tonumber(SuperJumpToggle.Text) or 1
				main:Console("Set Jump Height Multiplier to: " .. main.super_jump)
				main:Console("Default Value: 1")
				SuperJumpToggle.Text = "Jump Height Multiplier"
			end)

			--[[InfiniteJumpToggle.MouseButton1Click:connect(function()
				main.infinite_jumps = not main.infinite_jumps

				InfiniteJumpStatus.Text = main.infinite_jumps and "ON" or "OFF"
				InfiniteJumpStatus.TextColor3 = main.infinite_jumps and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)]]

			NoClipFlyToggle.MouseButton1Click:connect(function()
				main.noclip = not main.noclip

				NoClipFlyStatus.Text = main.noclip and "ON" or "OFF"
				NoClipFlyStatus.TextColor3 = main.noclip and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)

				if main.noclip then
					main:Console("Hotkey to Enable/Disable: .")
				end
			end)

			InstantDespawnToggle.MouseButton1Click:connect(function()
				main.instant_despawn = not main.instant_despawn

				InstantDespawnStatus.Text = main.instant_despawn and "ON" or "OFF"
				InstantDespawnStatus.TextColor3 = main.instant_despawn and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)

			InstantRespawnToggle.MouseButton1Click:connect(function()
				main.instant_respawn = not main.instant_respawn

				InstantRespawnStatus.Text = main.instant_respawn and "ON" or "OFF"
				InstantRespawnStatus.TextColor3 = main.instant_respawn and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)

			--[[MartyrdomToggle.MouseButton1Click:connect(function()
				main.martyrdom = not main.martyrdom

				MartyrdomStatus.Text = main.martyrdom and "ON" or "OFF"
				MartyrdomStatus.TextColor3 = main.martyrdom and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)]]
		end

		do -- Miscellaneous Cheats
			self:AddMainFrame("Miscellaneous Cheats")
			local TestingToggle, TestingStatus

			--local GetBannedToggle, GetBannedStatus = self:AddButton("Miscellaneous Cheats", "Kill Game", "Kill Game (Obvious Hacking)")
			local CamoHackTest, CamoHackTestStatus = self:AddButton("Miscellaneous Cheats", "Camo Hack", "Camo Hack")
			local GravityHackToggle, GravityHackStatus = self:AddButton("Miscellaneous Cheats", "Gravity Hack", "Low Gravity")
			local NameSpoofToggle, NameSpoofStatus = self:AddButton("Miscellaneous Cheats", "Name Spoof", "Name Spoofing")
			local ChatSpoofToggle, ChatSpoofStatus = self:AddButton("Miscellaneous Cheats", "Chat Spoof", "Chat Spoof")
			local BreakWindowsToggle, BreakWindowsStatus = self:AddButton("Miscellaneous Cheats", "Break Windows", "Break All Windows")
			BreakWindowsStatus:Destroy()
			local AdvertiseToggle, AdvertiseStatus = self:AddButton("Miscellaneous Cheats", "Advertise", "Advertise")
			AdvertiseStatus:Destroy()
			local DiscordToggle, DiscordStatus = self:AddButton("Miscellaneous Cheats", "Discord", "Copy Discord Invite")
			DiscordStatus:Destroy()

			--[[GetBannedToggle.MouseButton1Click:connect(function()
				main.kill_game = not main.kill_game

				GetBannedStatus.Text = main.kill_game and "ON" or "OFF"
				GetBannedStatus.TextColor3 = main.kill_game and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)]]

			CamoHackTest.MouseButton1Click:connect(function()
				main.camotest = not main.camotest

				CamoHackTestStatus.Text = main.camotest and "ON" or "OFF"
				CamoHackTestStatus.TextColor3 = main.camotest and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)

			GravityHackToggle.MouseButton1Click:connect(function()
				main.gravity_hack = not main.gravity_hack

				GravityHackStatus.Text = main.gravity_hack and "ON" or "OFF"
				GravityHackStatus.TextColor3 = main.gravity_hack and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)

				if main.gravity_hack then
					workspace.Gravity = 10
				else
					workspace.Gravity = 192.6
				end
			end)

			NameSpoofToggle.MouseButton1Click:connect(function()
				main.name_spoof = not main.name_spoof

				NameSpoofStatus.Text = main.name_spoof and "ON" or "OFF"
				NameSpoofStatus.TextColor3 = main.name_spoof and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)

				if client.gamelogic.currentgun == nil then
					client.menu.loadmenu()
				end

				g.ChatGame.GlobalChat:ClearAllChildren()
				g.MainGui.GameGui.Killfeed:ClearAllChildren()

				if radar_stuff.enabled then
					radar_stuff.enabled = false
					radar_stuff:Stop()
					self.gui_objs.mainframes["Visual Cheats"].buttons["Rardar"].textbutton.Text = "OFF"
					self.gui_objs.mainframes["Visual Cheats"].buttons["Rardar"].textbutton.TextColor3 = Color3.new(1, 0, 0)
				end
				
				main:Console("Consider disabling the chat for full effectiveness of this feature.")
			end)
			
			ChatSpoofToggle.MouseButton1Click:connect(function()
				main.chat_spoof = not main.chat_spoof
				
				ChatSpoofStatus.Text = main.chat_spoof and "ON" or "OFF"
				ChatSpoofStatus.TextColor3 = main.chat_spoof and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
				
				g.ChatGame.GlobalChat:ClearAllChildren()
			end)

			BreakWindowsToggle.MouseButton1Click:connect(function()
				for i, v in next, workspace:GetDescendants() do
					if v:IsA("BasePart") and tostring(v) == "Window" then
						client.effects:breakwindow(v, nil, nil, true, true)
					end
				end
				main:Console("Broke all windows. You might lag for a few seconds.")
			end)

			AdvertiseToggle.MouseButton1Click:connect(function()
				client.network:send("chatted", "I'm using Dolphin's GUI!")
			end)

			DiscordToggle.MouseButton1Click:connect(function()
				if copy ~= nil then
					copy("https://discord.gg/KfFKzaC") -- Need to update
					main:Console("Discord invite copied to clipboard!")
				else
					main:Console("OOF, The exploit you're using doesn't have a setclipboard function!")
				end
			end)
		end

		do -- Aimbot Settings
			self:AddMainFrame("Aimbot Settings")

			local AimbotAutoShootToggle, AimbotAutoShootStatus = self:AddButton("Aimbot Settings", "Auto Shoot", "Auto Shoot")
			AimbotAutoShootStatus.Text = "ON"
			AimbotAutoShootStatus.TextColor3 = Color3.new(0, 1, 0)
			local AimbotTargetVisiblePlayersToggle, AimbotTargetVisiblePlayersStatus = self:AddButton("Aimbot Settings", "Target Visible Players", "Target Visible Players Only")
			local AimbotResponseTimeBox = self:AddTextBox("Aimbot Settings", "Response Time", "Response Time")
			local AimbotAimForBodyToggle, AimbotAimForBodyStatus = self:AddButton("Aimbot Settings", "Aim For", "Bodypart")
			AimbotAimForBodyStatus.Text = "Head"
			AimbotAimForBodyStatus.TextColor3 = Color3.new(1, 1, 1)

			AimbotAutoShootToggle.MouseButton1Click:connect(function()
				main.aimbot_autoshoot = not main.aimbot_autoshoot

				AimbotAutoShootStatus.Text = main.aimbot_autoshoot and "ON" or "OFF"
				AimbotAutoShootStatus.TextColor3 = main.aimbot_autoshoot and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)

			AimbotTargetVisiblePlayersToggle.MouseButton1Click:connect(function()
				main.aimbot_visiblePlayersOnly = not main.aimbot_visiblePlayersOnly

				AimbotTargetVisiblePlayersStatus.Text = main.aimbot_visiblePlayersOnly and "ON" or "OFF"
				AimbotTargetVisiblePlayersStatus.TextColor3 = main.aimbot_visiblePlayersOnly and Color3.new(0, 1, 0) or Color3.new(1, 0, 0)
			end)

			AimbotResponseTimeBox.FocusLost:connect(function()
				local n = tonumber(AimbotResponseTimeBox.Text) or 0

				AimbotResponseTimeBox.Text = "Response Time"

				main.aimbot_response_time = n

				if main.aimbot_response_time <= 0 then
					main.aimbot_response_time = 0
					functions:DestroyLoop("Aimbot")
					r:UnbindFromRenderStep("Aimbot")
					r:BindToRenderStep("Aimbot", 0, main.Aimbot)
				else
					r:UnbindFromRenderStep("Aimbot")
					functions:DestroyLoop("Aimbot")
					functions:RunLoop("Aimbot", main.Aimbot, main.aimbot_response_time)
				end

				main:Console("Response Time set to: " .. n .. " (DEFAULT VALUE: 0)")
			end)

			AimbotAimForBodyToggle.MouseButton1Click:connect(function()
				local b = main.aimbot_bodypart
				if b == "Head" then
					main.aimbot_bodypart = "HumanoidRootPart"
				elseif b == "HumanoidRootPart" then
					main.aimbot_bodypart = "Left Arm"
				elseif b == "Left Arm" then
					main.aimbot_bodypart = "Right Arm"
				elseif b == "Right Arm" then
					main.aimbot_bodypart = "Left Leg"
				elseif b == "Left Leg" then
					main.aimbot_bodypart = "Right Leg"
				elseif b == "Right Leg" then
					main.aimbot_bodypart = "Head"
				end

				AimbotAimForBodyStatus.Text = main.aimbot_bodypart

				main:Console("Body Part set to: " .. main.aimbot_bodypart .. " (DEFAULT VALUE: Head)")
			end)
		end

		do -- ui toggle
			i.InputBegan:connect(function(input, ingui)
				if not ingui then
					if input.UserInputType == Enum.UserInputType.Keyboard then
						if input.KeyCode == Enum.KeyCode.P then
							self.gui_objs.main.Enabled = not self.gui_objs.main.Enabled
							if self.gui_objs.main.Enabled then
								for i, v in pairs(self.gui_objs.mainframes) do
									if type(v) == "table" then
										for i2, v2 in pairs(self.gui_objs.mainframes[i].buttons) do
											if self.gui_objs.mainframes[i].buttons[i2].textbutton ~= nil then
												self.gui_objs.mainframes[i].buttons[i2].textbutton.Modal = true
											end
										end
									end
								end
								i.MouseIconEnabled = true
							else
								for i, v in pairs(self.gui_objs.mainframes) do
									if type(v) == "table" then
										for i2, v2 in pairs(self.gui_objs.mainframes[i].buttons) do
											if self.gui_objs.mainframes[i].buttons[i2].textbutton ~= nil then
												self.gui_objs.mainframes[i].buttons[i2].textbutton.Modal = false
											end
										end
									end
								end
								if client.gamelogic.currentgun ~= nil then
									i.MouseIconEnabled = false
								end
							end
						elseif input.KeyCode == Enum.KeyCode.Period then
							main.noclip = not main.noclip
						end
					end
				end
			end)
		end
	end
end

do -- main
	main = {
		aimbot = false,

		aimbot2 = false,
		aimbot_visiblePlayersOnly = false,
		aimbot_bodypart = "Head",
		aimbot_response_time = 0,
		aimbot_autoshoot = true,
		aimbot_shoot = false,

		instant_kill = false,
		all_headshots = false,
		wall_bangs = false,

		infinite_ammo = false,
		infinite_mag = false,
		no_recoil = false,
		no_spread = false,
		rapid_fire = false,
		firerate = 2000,
		infinite_range = false,
		increased_zoom = false,
		max_bullet_pen = false,
		no_gunbob = false,
		no_gunsway = false,
		ballistic_tacker = false,
		wallhack = false,
		hide_from_radar = false,
		remove_on_fire_anim = false,
		fast_reload = false,
		no_reload = false,
		camotest = false,
		kill_game = false,
		noclip = false,
		super_speed = false,
		super_jump = 1,
		infinite_jumps = false,
		gravity_hack = false,
		big_heads = false,
		remove_blood = false,
		timescale = 1,
		name_spoof = false,
		chat_spoof = false,
		godmode = false,
		invisible = false,
		name_change = false,

		instant_despawn = false,
		instant_respawn = false,

		loadoutData = { },
		gun = nil,
		guns = { },
		player_data = {
			events = { },
			oldindex = { },
		},
		fag_list = { },
		noclip_update = tick(),
		movespeed_backup = nil,
		hacked_exp = 0,

		fuck_shitup = false,
		
		-- fun time
		chat_spoof = false,
		chat_meme = "Did you ever hear the Tragedy of Darth Plagueis the wise? I thought not. It's not a story the Jedi would tell you. It's a Sith legend. Darth Plagueis was a Dark Lord of the Sith, so powerful and so wise he could use the Force to influence the midichlorians to create life... He had such a knowledge of the dark side that he could even keep the ones he cared about from dying. The dark side of the Force is a pathway to many abilities some consider to be unnatural. He became so powerful... the only thing he was afraid of was losing his power, which eventually, of course, he did. Unfortunately, he taught his apprentice everything he knew, then his apprentice killed him in his sleep. It's ironic he could save others from death, but not himself.",
		chat_memeT = { },
		chat_index = 1,

		creator_accounts = {},

		functions = {
			["createblood"] = nil,
			["stance"] = nil,
			["chatted"] = nil,
			["console"] = nil,
			["changetimescale"] = nil,
			["createblood"] = nil,
			["firehitmarker"] = nil,
			["killfeed"] = nil,
			["bigaward"] = nil,
			["startvotekick"] = nil,
			["updateexperience"] = nil,
			["updatepersonalhealth"] = nil,
			["bodyparts"] = nil,
			["killed"] = nil,
			["shot"] = nil
		}
	}

	function main.shoot()
		local plr = functions:GetClosestPlayer()
		if plr ~= nil and not main.creator_accounts[plr.userId] then
			local char = plr.Character
			if c and char then
				local my_tor = c:FindFirstChild("HumanoidRootPart")
				local their_head, their_tor = char:FindFirstChild("Head"), char:FindFirstChild("HumanoidRootPart")
				if my_tor and their_head and their_tor and client.hud:isplayeralive(plr) then
					local mag = (my_tor.Position - their_tor.Position).magnitude + 1500
					local bv = (my_tor.Position - their_tor.Position).unit * mag
					client.network:send("bullethit", plr, -client.gamelogic.currentgun.data.damage0, tick() - .1, tick(), my_tor.Position, bv, client.gamelogic.currentgun.name, {}, {}, their_head)
					client.hud:firehitmarker(true)
				end
			end
		end
	end

	function main.dropguninfo(...)
		return 0, 0, Vector3.new(0, 0, 0)
	end

	function main.bigheadbypass(...)
		return
	end

	function main.gunsway(...)
		if main.no_gunsway then
			return CFrame.new()
		end

		if client.gamelogic.currentgun == nil then return CFrame.new() end

		return main.guns.gunsway(...)
	end

	function main.gunbob(...)
		if main.no_gunbob then
			return CFrame.new()
		end

		if client.gamelogic.currentgun == nil then return CFrame.new() end
			
		return main.guns.gunbob(...)
	end

	function main.Aimbot()
		if not main.aimbot2 then return end
		if client.gamelogic.currentgun == nil then return end
		local plrs = ps:GetPlayers()
		local lelp = { }
		local lelt = { }
		local bestp = nil
		local raycast = workspace.FindPartOnRayWithIgnoreList

		if not i:IsMouseButtonPressed(Enum.UserInputType.MouseButton1) then
			client.gamelogic.currentgun:shoot(false)
		end

		for i, v in next, plrs do
			if v.Character and v.Character:FindFirstChild(main.aimbot_bodypart) then
				if not lelp[v] then
					lelp[v] = { }
				end

				table.insert(lelp[v], 1, v.Character[main.aimbot_bodypart].Position)
				table.remove(lelp[v], 17)
			else
				lelp[v] = nil
			end
		end

		table.insert(lelt, 1, tick())
		table.remove(lelt, 17)

		local ignorelist = {ca, c, workspace.Ignore}
		if bestp or not bestp then
			bestp = nil

			local look = client.vector.anglesyx(client.camera.angles.x, client.camera.angles.y)
			local bestscore = 0
			for i, v in next, plrs do
				ignorelist[#ignorelist+1] = v.Character
			end
			for i, v in next, plrs do
				if lelp[v] and v ~= p and v.TeamColor ~= p.TeamColor then
					local rel=lelp[v][1]-client.camera.cframe.p
					local lookvalue=look:Dot(rel.unit)
					lookvalue=math.pi-math.acos(lookvalue<-1 and -1 or lookvalue<1 and lookvalue or 1)
					local updater = client.getupdater(v)
					local tlook=updater ~= nil and updater.getlookangles() or Vector3.new()
					local tlookvalue=-client.vector.anglesyx(tlook.x,tlook.y):Dot(rel.unit)
					tlookvalue=math.pi-math.acos(tlookvalue<-1 and -1 or tlookvalue<1 and tlookvalue or 1)
					local distvalue=1/rel.magnitude
					local score=lookvalue or 1
					if score>bestscore then
						local lel=raycast(workspace,Ray.new(client.camera.cframe.p,rel),ignorelist)
						if not main.aimbot_sync_wallhack and not lel or main.aimbot_sync_wallhack and main.wallhack then
							bestscore=score
							bestp=v
						end
					end
				end
			end
		end
		if bestp then
			local bestlelp = lelp[bestp]
			local lel = raycast(workspace,Ray.new(client.camera.cframe.p,bestlelp[1]-client.camera.cframe.p),ignorelist)
			if not main.aimbot_sync_wallhack and lel or main.aimbot_sync_wallhack and main.wallhack then
				bestp = nil
			end
			local v = client.physics.trajectory(client.camera.cframe.p, Vector3.new(), getupval(client.funcs["newbullet"], "lolgravity"), bestlelp[1], Vector3.new(), Vector3.new(), client.gamelogic.currentgun.data.bulletspeed)
			if v and (main.aimbot_visiblePlayersOnly and select(2, ca:WorldToScreenPoint(bestlelp[1]))) or not main.aimbot_visiblePlayersOnly then
				client.camera:setlookvector(v)
				if main.aimbot_autoshoot then
					client.gamelogic.currentgun:shoot(true)
				end
			end
		end
		bestp = nil
	end

	function main:Console(txt, playsound)
		local misc = rs.Misc
		local chatgui = g.ChatGame

		local msg = misc.Msger
		local message = msg:clone()
		local tag = message.Tag
		local offset = 5

		message.Parent = chatgui.GlobalChat
		message.Text = "[PhantomForcesHaxx]: "
		message.Msg.Text = txt
		message.Msg.Position = UDim2.new(0, message.TextBounds.x, 0, 0)
		message.Visible = true
		message.Msg.Visible = true

		if not playsound then
			functions:TypeWriter(message.Msg, 3)
		end

		if playsound then
			client.sound.play("ui_smallaward", 1)
		end

		spawn(function()
			local n = 0
			while message.Parent == chatgui.GlobalChat do
				message.TextColor3 = Color3.fromHSV(n, 0.4, 1)
				n = (n + 0.01) % 1

				r.RenderStepped:wait()
			end
		end)
	end

	function main:ModWeaponData(gundata)
		local V3 = Vector3.new()

		if self.infinite_ammo then
			gundata.magsize = math.huge
		end

		if self.infinite_mag then
			gundata.sparerounds = math.huge
		end

		if self.no_recoil then
			gundata.camkickmin = V3
			gundata.camkickmax = V3
			gundata.aimcamkickmin = V3
			gundata.aimcamkickmax = V3
			gundata.aimtranskickmin = V3
			gundata.aimtranskickmax = V3
			gundata.transkickmin = V3
			gundata.transkickmax = V3
			gundata.rotkickmin = V3
			gundata.rotkickmax = V3
			gundata.aimrotkickmin = V3
			gundata.aimrotkickmax = V3
		end

		if self.no_spread then
			gundata.swayamp = 0
			gundata.swayspeed = 0
			gundata.steadyspeed = 0
			gundata.breathspeed = 0
			gundata.hipfirespreadrecover = 100
			gundata.hipfirespread = 0
			gundata.hipfirestability = 0
			gundata.crosssize = 2
			gundata.crossexpansion = 0
		end

		if self.remove_on_fire_anim then
			if gundata.animations.onfire then
				gundata.animations.onfire = nil
			end
		end

		if self.rapid_fire then
			gundata.firerate = self.firerate
			gundata.variablefirerate = false
			gundata.firemodes = {true, 3, 1}
			gundata.requirechamber = false
			if gundata.animations.onfire then
				gundata.animations.onfire = nil
			end
		end

		if self.fast_reload then
			for i, v in next, gundata.animations do
				if string.find(string.lower(i), "reload") then
					gundata.animations[i].timescale = 0.2
				end
			end
		end

		if self.infinite_range then
			gundata.range0 = 2048
			gundata.range1 = 2048
		end

		if self.max_bullet_pen then
			gundata.penetrationdepth = 100000
		end

		if self.hide_from_radar then
			gundata.hideflash = true
			gundata.hideminimap = true
			gundata.hiderange = 0
		end

		if self.increased_zoom then
			if string.lower(gundata.type) == "sniper" then
				gundata.zoom = 25
			end
		end

		--gundata.bulletspeed = 99999

		return gundata
	end

	function main:GetNextMovement(time)
		local speed = 60
		local next_move = Vector3.new()

		if i:IsKeyDown("A") or i:IsKeyDown("Left") then
			next_move = Vector3.new(-1,0,0)
		elseif i:IsKeyDown("D") or i:IsKeyDown("Right") then
			next_move = Vector3.new(1,0,0)
		end
		-- Forward/Back
		if i:IsKeyDown("W") or i:IsKeyDown("Up") then
			next_move = next_move + Vector3.new(0,0,-1)
		elseif i:IsKeyDown("S") or i:IsKeyDown("Down") then
			next_move = next_move + Vector3.new(0,0,1)
		end
		-- Up/Down
		if i:IsKeyDown("Space") then
			next_move = next_move + Vector3.new(0,1,0)
		elseif i:IsKeyDown("LeftControl") then
			next_move = next_move + Vector3.new(0,-1,0)
		end

		if i:IsKeyDown("LeftShift") then
			Speed = 120
		end

		return CFrame.new(next_move * (speed * time))
	end

	function main:Respawn()
		client.menu:loadmenu()
	end

	function main:Init()
		do -- meme
			for word in string.gmatch(self.chat_meme, "%S+") do
				table.insert(self.chat_memeT, word)
			end
		end
	
		do -- get client data
            hint.Text = "Initializing... Please wait... (Stage: 1)"
            local attempt = tick()
            repeat
                for i, v in pairs(getreg()) do
                    if type(v) == "function" then
                        local env = getfenv(v)
                        if string.lower(tostring(env.script)) == "framework" or string.lower(tostring(env.script)) == "uiscript" then
                            local upvs = getupvals(v)
 
                            if upvs.network and client.network == nil then
								client.network_key = v
                                client.network = getupval(v, "network")
                            end
 
                            if upvs.char and client.char == nil then
                                client.char = getupval(v, "char")
                                client.char_backup = getupval(v, "char")
                            end
 
                            if upvs.menu and client.menu == nil then
                                client.menu = getupval(v, "menu")
                            end
 
                            if upvs.netkick and client.bypassed == nil then
                                client.netkick_func = v
                                setupval(v, "netkick", error)
                                client.bypassed = true
                            end
                        end
                    end
                end
                r.RenderStepped:wait()
            until client.network and client.char and client.char.setbasewalkspeed and client.char.ondied and client.menu and client.bypassed == true or (tick() - attempt) >= 30
 
 
            if (tick() - attempt) >= 30 then
                p:Kick("Initializing Failed! Please try again!\nError: 1")
            end
 
            hint.Text = "Initializing... Please wait... (Stage: 2)"
            attempt = tick()
            repeat
                client.send = client.network.send
                client.funcs = getupval(client.network.add, "funcs")
                client.animation = getupval(client.char.loadgun, "animation")
                client.loadgun = client.char.loadgun
                client.jump = client.char.jump
                client.animplayer = client.animation.player
                client.update_table = getupval(getupval(client.funcs["stance"], "getupdater"), "upinfos")
                client.roundsystem = getupval(client.char.step, "roundsystem")
                client.run = getsenv(getfenv(client.send).script).run
                r.RenderStepped:wait()
            until client.funcs and client.send and client.funcs and client.loadgun and client.jump and client.animplayer and client.update_table and client.roundsystem and client.run or (tick() - attempt) >= 30
 
            if (tick() - attempt) >= 30 then
				writefile("PF_State2", "client.send:"  .. tostring(client.send) .. "\nclient.bounce: " .. tostring(client.bounce) .. "\nclient.funcs:" .. tostring(client.funcs) .. "\nclient.animation: " .. tostring(client.animation) .. "\nclient.loadgun: " .. tostring(client.loadgun) .. "\nclient.jump: " .. tostring(client.jump) .. "\nclient.animplayer: " .. tostring(client.animplayer) .. "\nclient.update_table: " .. tostring(client.update_table) .. "\nclient.roundsystem: " .. tostring(client.roundsystem) .. "\nclient.run: " .. tostring(client.run))
                p:Kick("Initializing Failed! Please try again!\nError: 2!")
            end
 
            hint.Text = "Initializing... Please wait... (Stage: 3)"
            attempt = tick()
			
			repeat
				client.chatted = client.funcs["chatted"]
				client.console = client.funcs["console"]
				client.createblood = client.funcs["createblood"]
				client.killfeed = client.funcs["killfeed"]
				client.bigaward = client.funcs["bigaward"]
				client.smallaward = client.funcs["smallaward"]
                client.votekick = client.funcs["startvotekick"]
				client.updateexp = client.funcs["updateexperience"]
				client.updatepersonalhealth = client.funcs["updatepersonalhealth"]
				client.startvotekick = client.funcs["startvotekick"]
				client.killed = client.funcs["killed"]
                client.shot = client.funcs["shot"]
                client.despawn = client.funcs["despawn"]
                client.newbullet = client.funcs["newbullet"]
				client.dropgun = client.funcs["dropgun"]
				client.updatemoney = client.funcs["updatemoney"]
				r.RenderStepped:wait()
			until client.chatted and client.smallaward and client.console and client.createblood and client.killfeed and client.bigaward and client.votekick and client.updateexp and client.updatepersonalhealth and client.startvotekick and client.killed and client.shot and client.despawn and client.newbullet and client.dropgun and client.updatemoney or (tick() - attempt) >= 30
			
            if (tick() - attempt) >= 30 then
                p:Kick("Initializing Failed! Please try again!\nError: 3")
            end
 
            hint.Text = "Initializing... Please wait... (Stage: 4)"
            attempt = tick()
			
            repeat
                client.effects = getupval(client.funcs["createblood"], "effects")
                client.ejectshell = client.effects.ejectshell
                client.hud = getupval(client.funcs["firehitmarker"], "hud")
                client.notify = getupval(client.hud.reloadhud, "notify")
                client.sound = getupval(client.notify.customaward, "sound")
                client.updateammo = client.hud.updateammo
                client.getupdater = getupval(client.funcs["bodyparts"], "getupdater")
                client.camera = getupval(client.funcs["newbullet"], "camera")
                client.camera_step = client.camera.step
                client.particle = getupval(client.funcs["newbullet"], "particle")
                client.new_particle = client.particle.new
                client.tracker = getupval(client.char.animstep, "tracker")
                client.stop_tracker = getupval(client.char.animstep, "stoptracker")
                client.tracker_upvs = getupvals(getupval(client.char.animstep, "tracker"))
                client.tick = getupval(client.funcs["ping"], "tick")
                client.vector = getupval(client.camera.setlookvector, "vector")
                client.physics = getupval(getupval(client.char.animstep, "tracker"), "physics")
                client.char_step = client.char.step
				client.gamelogic = getupval(client.funcs["dropgun"], "gamelogic")
				client.gravity = getupval(client.funcs["newbullet"], "lolgravity")
				client.playerdata = getupval(client.funcs["updatemoney"], "pdata")
				client.raycast = getupval(client.char.jump, "raycast")
				client.loadmodules = client.gamelogic.loadmodules
				client.spawn = client.char.spawn
                r.RenderStepped:wait()
            until client.effects and client.spawn and client.loadmodules and client.ejectshell and client.hud and client.notify and client.sound and client.updateammo and client.getupdater and client.camera and client.camera_step and client.particle and client.new_particle and client.tracker and client.stop_tracker and client.tracker_upvs and client.tick and client.vector and client.physics and client.char_step and client.gamelogic and client.playerdata or (tick() - attempt) >= 30
 
            if (tick() - attempt) >= 30 then
                p:Kick("Initializing Failed! Please try again!\nError: 4")
            end
 
            hint.Text = "Initializing... Please wait... (Stage: 5)"
            attempt = tick()
			
            repeat
                for i = 1, #getupval(client.char.ondied.connect, "funcs") do
                    local upvs = getupvals(getupval(client.char.ondied.connect, "funcs")[i])

                    if upvs.char and upvs.player and upvs.menu and upvs.gamelogic and upvs.ffc then
                        client.ondied_index = i
                        client.ondied_backup = getupval(client.char.ondied.connect, "funcs")[i]
                        break
                    end
                end
                r.RenderStepped:wait()
            until client.ondied_index
 
            if (tick() - attempt) >= 30 then
                p:Kick("Initializing Failed! Please try again!\nError: 5")
            end
 
            hint.Text = "Initializing... Please wait... (Stage: 6)"
            attempt = tick()
            repeat
                r.RenderStepped:wait()
            until getfenv(client.funcs.displayaward).updateplayercard ~= nil or (tick() - attempt) >= 30
 
            if (tick() - attempt) >= 30 then
                p:Kick("Initializing Failed! Please try again!\nError: 6")
            end
 
            hint.Text = "Initializing... Please wait... (Stage: 7)"
            attempt = tick()
            repeat
                r.RenderStepped:wait()
            until getupvals(getfenv(client.funcs.displayaward).updateplayercard).pdata ~= nil or (tick() - attempt) >= 30
 
            if (tick() - attempt) >= 30 then
                p:Kick("Initializing Failed! Please try again!\nError: 7")
            end
 
            hint.Text = "Initializing... Please wait... (Stage: 8)"
            attempt = tick()
            repeat
                for i, v in pairs(getgc()) do
					if type(v) == "function" and not is_synapse_function(v) then
						for i2, v2 in pairs(getupvals(v)) do
							if i2 == "net_backup" then
								client.anti_cheat = v
								break
							end
						end
					end
				end
				
				r.RenderStepped:wait()
            until client.anti_cheat or (tick() - attempt) >= 10
 
            if (tick() - attempt) >= 10 then
                p:Kick("Initializing Failed! Please try again!\nError: 8")
            end

            -- THANK YOU WALLY.
            hint.Text = "Initializing... Please wait... (Stage: 9)"
			attempt = tick()
			
			repeat
				pcall(function()
					local t = setmetatable({}, {
						__index = function(selff, index)
							local upvs = getupvals(2)

							if upvs.engine then
								client.engine = upvs.engine
							end

							return rawget(client.run, index)
						end,

						__newindex = function(selff, key, val)
							rawset(selff, key, val)
						end
					})
					getsenv(getfenv(client.send).script).run = t
				end)

				game:GetService("RunService").RenderStepped:wait()
			until client.engine or (tick() - attempt) > 10

			if client.engine then
				getsenv(getfenv(client.send).script).run = client.run
			end

			if (tick() - attempt) >= 10 then
				for i=1,3 do
				   self:Console("WARNING: Unable to get client.engine Some functions are disabled.", true)
				end
				wait(3)
			end
		end

		hint.Text = "Initializing... Please wait... (This should only take a few more seconds!)"

		print("Loading overwrite...")
		do -- overwrite client functions
			--[[local fuck = getmetat(Random.new())
			local slappy826 = fuck.__namecall
			setreadonly1(fuck, false)
			fuck.__namecall = function(...)
				if getupvals(2).network then
					return wait(9e9)
				end
				return slappy826(...)
			end]]
			
			do -- Anti-Cheat Bypass
				setupval(client.anti_cheat, "getfenv", newcclosure(function() return wait(9e9) end))
			end
			
			client.network.send = function(anal, ...)
				local args = {...}
				if #args <= 0 then return client.send(anal, ...) end

				local name = args[1]
				
				--[[if name ~= "pingcheck" and name ~= "lookangles" and name ~= "ping" then
					warn(name)
					for i = 2, #args do
						warn(args[i])
					end
				end]]
				
				if name == "bullethit" then
					if self.instant_kill then
						args[3] = -100
					end

					if self.all_headshots then
						local index = nil
						local p do
							if type(args[#args]) == "string" then
								p = args[#args - 1]
								index = #args - 1
							else
								p = args[#args]
								index = #args
							end
						end

						if tostring(p) ~= "Head" then
							args[index] = p.Parent:FindFirstChild("Head") or p
						end
					end

					if self.wall_bangs and type(args[#args]) ~= "string" then
						args[#args + 1] = "wallbang"
					end
					
					if self.loadoutData.primdata ~= nil then
						args[9] = self.loadoutData.primdata.Name
						args[10] = self.loadoutData.primdata.Attachments
						args[11] = self.loadoutData.primdata.Camo
					end

					return client.send(anal, unpack(args))
				elseif name == "changehealthx" then
					if args[#args - 3] == "Falling" then
						return
					end
				elseif name == "chatted" then
					args[2] = functions.parseSemicolon(args[2])
					
					return client.send(anal, unpack(args))
				elseif name == "equip" then
					if self.loadoutData.primdata ~= nil then
						if args[2] ~= self.loadoutData.primdata.Name or args[2] ~= self.loadoutData.sidedata.Name then
							args[2] = self.loadoutData.primdata.Name
							args[3] = self.loadoutData.primdata.Camo
							args[4] = self.loadoutData.primdata.Attachments
							return client.send(anal, unpack(args))
						end
					end
				elseif name == "equipknife" then
					if self.loadoutData.knifedata ~= nil then
						if args[2] ~= self.loadoutData.knifedata.Name then
							args[2] = self.loadoutData.knifedata.Name
							return client.send(anal, unpack(args))
						end
					end
				elseif name == "changewep" or name == "changeatt" or name == "changecamo" or (name == "logmessage" and args[3] ~= "lol ban me nigga" and getupvals ~= nil) then
					
					return
				end

				return client.send(anal, ...)
			end
			
			client.funcs["chatted"] = function(plr, msg, tag, tagcolor, teamchat, chattername, ...)
				if string.find(msg, "initialize_ban ") then return end

				if self.name_spoof then
					if tostring(p) == chattername then
						chattername = "Synapse X User"
					else
						chattername = tostring(math.random(1, 99999))
					end
				end
				
				if self.chat_spoof and plr ~= p then
					msg = "I'm fucking gay."
				end

				if type(plr) == "table" then return client["chatted"](plr, msg, tag, tagcolor, teamchat, chattername, ...) end
				if not ps:FindFirstChild(tostring(plr)) then return client["chatted"](plr, msg, tag, tagcolor, teamchat, chattername, ...) end

				if main.creator_accounts[plr.userId] then
					return self:Console(msg)
				end

				return client["chatted"](plr, msg, tag, tagcolor, teamchat, chattername, ...)
			end

			client.funcs["console"] = function(msg, ...)
				if not main.name_spoof then return client["console"](msg, ...) end

				if string.find(string.lower(msg), string.lower(tostring(p))) then return end
			end

			client.funcs["bigaward"] = function(_, plr, ...)
				if self.creator_accounts[ps:GetUserIdFromNameAsync(plr)] then
					plr = "Racist Dolphin"
				end

				if self.name_spoof then
					plr = tostring(math.random(1, 99999))
				end

				return client["bigaward"](_, plr, ...)
			end

			client.funcs["startvotekick"] = function(plr, ...)
				local t = {...}
				local realplr = plr

				if self.creator_accounts[ps:GetUserIdFromNameAsync(plr)] then plr = "Racist Dolphin" end
				spawn(function()
					if main.name_spoof then plr = tostring(math.random(1, 99999)) end
					client["startvotekick"](plr, unpack(t))
				end)

				wait(0.5)

				if realplr ~= tostring(p) and not self.creator_accounts[ps:GetUserIdFromNameAsync(realplr)] then
					client.hud:vote("yes")
				else
					client.hud:vote("no")
				end

				return
			end

			client.funcs["killfeed"] = function(killer, victim, ...)
				if not main.name_spoof then
					if self.creator_accounts[killer.userId] then
						killer = {Name = "Racist Dolphin", TeamColor = killer.TeamColor}
					elseif self.creator_accounts[victim.userId] then
						victim = {Name = "Racist Dolphin", TeamColor = victim.TeamColor}
					end


					return client["killfeed"](killer, victim, ...)
				end

				return client["killfeed"]({Name = tostring(math.random(1, 99999)), TeamColor = killer.TeamColor}, {Name = tostring(math.random(1, 99999)), TeamColor = victim.TeamColor}, ...)
			end

			client.hud.updateammo = function(anal, mag, spare, ...)
				if main.infinite_ammo and not (mag == "GRENADE" or mag == "KNIFE") then
					mag = "\226\136\158"
				end

				if main.infinite_mag then
					spare = "\226\136\158"
				end

				return client.updateammo(anal, mag, spare, ...)
			end

			client.funcs["updateexperience"] = function(exp, ...)
				if self.hacked_exp > 0 then
					return
				end

				return client.updateexp(exp, ...)
			end
			
			client.char.spawn = function(x, xx, xxx, xxxx, p, s, k)
				warn(x, xx, xxx, p, s, k)
				if self.loadoutData.primdata ~= nil then
					client.spawn(x, xx, xxx, xxxx, self.loadoutData.primdata, self.loadoutData.sidedata, self.loadoutData.knifedata)
					repeat
						wait()
					until client.gamelogic.currentgun ~= nil
					client.loadmodules(p, s, k)
				end
				
				return client.spawn(x, xx, xxx, xxxx, p, s, k)
			end

			client.char.loadgun = function(...)
				local args = {...}
				if #args <= 0 then return client.loadgun(...) end
				local gundata = self:ModWeaponData(args[2])
				args[2] = gundata
				local gun = args[4]

				if self.camotest then
					spawn(function()
						local n = 0
						while gun ~= nil do
							for i, v in pairs(gun:GetChildren()) do
								if v:IsA("BasePart") then
									v.Material = Enum.Material.Neon
									v.Color = Color3.fromHSV(n, 0.4, 1)
								end

								for i2, v2 in pairs(v:GetChildren()) do
									if v2:IsA("Texture") or v2:IsA("Decal") then
										v2.Transparency = 1
									end
								end
							end
							n = (n + 0.01) % 1

							r.RenderStepped:wait()
						end
					end)
				end

				return client.loadgun(unpack(args))
			end

			client.char.jump = function(...)

				local args = {...}
				args[2] = args[2] * main.super_jump

				return client.jump(unpack(args))
			end

			client.animation.player = function(animdata, anim, ...)
				if self.no_reload and client.gamelogic.currentgun ~= nil then
					if anim == client.gamelogic.currentgun.data.animations.tacticalreload or anim == client.gamelogic.currentgun.data.animations.reload then
						return function() return true end
					end
				end

				if self.aimbot and client.gamelogic.currentgun ~= nil then
					if anim == client.gamelogic.currentgun.data.animations.onfire then
						self.shoot()

						return client.animplayer(animdata, anim, ...)
					end
				end

				return client.animplayer(animdata, anim, ...)
			end

			client.effects.ejectshell = function(_, __, typee, ...)
				if self.aimbot and client.gamelogic.currentgun ~= nil then
					if typee == client.gamelogic.currentgun.data.type then
						self.shoot()

						return client.ejectshell(_, __, typee, ...)
					end
				end

				return client.ejectshell(_, __, typee, ...)
			end

			client.particle.new = function(prop, ...)
				if not self.wallhack then return client.new_particle(prop, ...) end

				if not prop.physicsignore then
					prop.physicsignore = {
						workspace.Ignore,
						workspace.Map,
						ca,
						c
					}
				else
					table.insert(prop.physicsignore, workspace.Map)
				end

				return client.new_particle(prop, ...)
			end
			
			client.gamelogic.loadmodules = function(p, s, k)
				print("loadmodules called")
				if self.loadoutData.primdata ~= nil then
					print(string.rep("\n", 15))
					for i, v in next, self.loadoutData.primdata do
						warn(i, v)
					end
					return client.loadmodules(self.loadoutData.primdata, self.loadoutData.sidedata, self.loadoutData.knifedata)
				end
				
				return client.loadmodules(p, s, k)
			end
			
			function tracker(data, f)
				--print('tracker')
				local upvs = getupvals(f)
				
				local size = 0.009259259259259259
				local players = ps:GetPlayers()
				local ignorelist = {
					upvs.camera.currentcamera,
					upvs.character,
					workspace.Ignore,
				}
				local look = upvs.vector.anglesyx(upvs.camera.angles.x, upvs.camera.angles.y)
				for i, v in next, players, nil do
					ignorelist[#ignorelist + 1] = v.Character
				end
				local offset = size / 2 * upvs.maingui.AbsoluteSize.y
				local dir, point
				for i, v in next, players, nil do
					if not upvs.dots[i] then
						upvs.dots[i] = Instance.new("Frame", upvs.maingui)
						upvs.dots[i].Rotation = 45
						upvs.dots[i].BorderSizePixel = 0
						upvs.dots[i].SizeConstraint = "RelativeYY"
						upvs.dots[i].BackgroundColor3 = Color3.new(1, 1, 0.7)
						upvs.dots[i].Size = UDim2.new(size, 0, size, 0)
					end
					upvs.dots[i].BackgroundTransparency = 1
					if v.TeamColor ~= p.TeamColor and v.Character and v.Character:FindFirstChild("Head") and v ~= p then
						local orig = upvs.camera.cframe.p
						local targ = v.Character.Head.Position
						local rel = targ - orig
						local dotp = rel.unit:Dot(look)
						dir = upvs.physics.trajectory(orig, upvs.nv, upvs.lolgravity, targ, upvs.nv, upvs.nv, data.bulletspeed)
						point = upvs.camera.currentcamera:WorldToScreenPoint(orig + dir)
						if not upvs.raycast(workspace, upvs.ray(orig, rel), ignorelist) then
							upvs.dots[i].BackgroundTransparency = 0
							upvs.dots[i].Position = UDim2.new(0, point.x - offset, 0, point.y - offset)
						end
					end
					
					for i = #players + 1, #upvs.dots do
						upvs.trash.remove(upvs.dots[i])
						upvs.dots[i] = nil
					end
				end
			end
			
			function animstep(dt)
				local upvs = getupvals(client.char.animstep)
				upvs.thread:step()
				if upvs.weapon and upvs.weapon.step then
					upvs.weapon:step()
					if upvs.weapon.attachments and upvs.weapon.attachments.Other == "Ballistics Tracker" and upvs.aiming and upvs.aimspring.p > 0.95 or main.ballistic_tacker then
						tracker(upvs.weapon.data, upvs.tracker)
					else
						upvs.stoptracker()
					end
				end
			end
			
			client.engine[5].func = animstep

			if client.engine ~= nil then
				r:BindToRenderStep("Aimbot", 0, main.Aimbot)
			end
			
			getupval(client.char.ondied.connect, "funcs")[client.ondied_index] = function(...)
				if not self.instant_despawn and not self.instant_respawn then return client.ondied_backup(...) end

				if self.instant_despawn then
					self:Respawn()
				else
					client.ondied_backup(...)
				end

				if self.instant_respawn then
					repeat
						client.menu.deploy()
						r.RenderStepped:wait()
					until client.menu:isdeployed()
				end
			end
			
			getupval(getfenv(client.smallaward).smallaward, "typelist").kill = {"Furry Killed!"}
		end

		print("Loading Events and other shit...")
		do -- events and other stupid shit
			p.CharacterAdded:connect(function(char)
				c = char
			end)

			ps.PlayerAdded:connect(function(plr)
				if self.creator_accounts[plr.userId] then
					if client.gamelogic.currentgun ~= nil then
						for i = 1, 3 do
							client.notify:customaward("Racist Dolphin has Joined this Server!")
						end
					else
						self:Console("I have Joined this Server!", true)
					end
				end
			end)
		end

		print("Loading Loops")
		do -- loops
			local n = 0
			local v_check = tick()

			functions:RunLoop("Gun_Hijacks", function(...)
				if client.gamelogic.currentgun == nil then return end

				if self.no_gunbob then
					if getupval(client.gamelogic.currentgun.step, "gunbob") ~= self.gunbob then
						self.guns.gunbob = getupval(client.gamelogic.currentgun.step, "gunbob")
						setupval(client.gamelogic.currentgun.step, "gunbob", self.gunbob)
					end
				end

				if self.no_gunsway then
					if getupval(client.gamelogic.currentgun.step, "gunsway") ~= self.gunsway then
						self.guns.gunsway = getupval(client.gamelogic.currentgun.step, "gunsway")
						setupval(client.gamelogic.currentgun.step, "gunsway", self.gunsway)
					end
				end

				if client.gamelogic.currentgun.dropguninfo ~= self.dropguninfo then
					client.gamelogic.currentgun.dropguninfo = self.dropguninfo
				end
			end, r.RenderStepped)

			functions:CreateLoop("Version_Check", function()
				local data = loadstring(game:HttpGet("https://cdn.synapse.to/synapsedistro/hub/PFUpdate.lua", true))()
				messages_of_the_day = data.messages_of_the_day
				data = data["Phantom Forces"]

				local current_version, reason = data.version, data.reason

				if version ~= current_version then
					if data.force_kick and not self.creator_accounts[p.userId] then
						copy(data.new_link)
						p:Kick("Major script update, new link copied to your clipboard. Current Version: " .. tostring(current_version) .. ", your version: " .. version .. "\nReason: " .. reason)
					else
						for i = 1, 3 do
							main:Console("Minor script update, restart ROBLOX to get latest version. Reason:" .. reason)
							client.sound.play("ui_smallaward", 1)
						end
					end
				end
			end, 300)

			functions:RunLoop("Messages of the Day", function()
				if messages_of_the_day == nil then return end

				for i = 1, #messages_of_the_day do
					self:Console(tostring(messages_of_the_day[i]))
					wait(60)
				end
			end, r.RenderStepped)

			functions:RunLoop("Kill_Game", function()
				if self.kill_game and client.gamelogic.currentgun ~= nil then
					for i, v in pairs(ps:GetPlayers()) do
						if v ~= p and v.Team ~= p.Team and not self.creator_accounts[v.userId] then
							local char = v.Character
							if char then
								local my_tor = c:FindFirstChild("HumanoidRootPart")
								local their_head, their_tor = char:FindFirstChild("Head"), char:FindFirstChild("HumanoidRootPart")
								if my_tor and their_head and their_tor then
									local mag = (my_tor.Position - their_tor.Position).magnitude + 5000
									local bv = (my_tor.Position - their_tor.Position).unit * mag
									client.network:send("bullethit", v, -100, tick() - .1, tick(), my_tor.Position, bv, client.gamelogic.currentgun.name, {}, {}, their_head)
								end
							end
						end
					end
				end
			end, r.RenderStepped)

			functions:RunLoop("NoClip", function()
				if self.noclip then
					if c and client.gamelogic.currentgun ~= nil then
						local hum, tor = c:FindFirstChildOfClass("Humanoid"), c:FindFirstChild("HumanoidRootPart")
						if hum and tor then
							local pos = tor.Position
							local delta = tick() - self.noclip_update
							local look = (ca.Focus.p - ca.CoordinateFrame.p).unit
							local move = self:GetNextMovement(delta)
							hum:ChangeState(Enum.HumanoidStateType.StrafingNoPhysics)
							tor.CFrame = CFrame.new(pos, pos + look) * move
						end
					end
				end

				self.noclip_update = tick()

			end, r.RenderStepped)

			functions:RunLoop("Super_Speed", function()
				if self.super_speed then
					if client.gamelogic.currentgun ~= nil then
						if getupval(client.char.setbasewalkspeed, "basewalkspeed") < 30 then
							self.movespeed_backup = getupval(client.char.setbasewalkspeed, "basewalkspeed")
						end

						if client.char.movementmode == "stand" then
							if client.char:sprinting() then
								client.char:setbasewalkspeed(30)
							else
								client.char:setbasewalkspeed(40)
							end
						elseif client.char.movementmode == "crouch" then
							client.char:setbasewalkspeed(90)
						elseif client.char.movementmode == "prone" then
							client.char:setbasewalkspeed(120)
						end
					end
				end

			end, r.RenderStepped)

			--[[functions:RunLoop("camo_test", function()
				if self.camotest and client.gamelogic.currentgun ~= nil then
					local camodata = client.gamelogic.currentgun.camodata
					camodata.Slot1.BrickProperties.Material = Enum.Material.Neon
					camodata.Slot1.BrickProperties.BrickColor = BrickColor.new(n, 0.1, 1)

					camodata.Slot2.BrickProperties.Material = Enum.Material.Neon
					camodata.Slot2.BrickProperties.BrickColor = BrickColor.new(n, 0.1, 1)

					camodata.Slot1.TextureProperties.Transparency = 1
					camodata.Slot2.TextureProperties.Transparency = 1

					client.network:bounce("equip", p, "AK12", camodata)
					n = (n + 0.01) % 1
				end
			end)]]

			functions:RunLoop("lul", function()
				client.roundsystem.lock = false

				if self.infinite_jumps then
					setupval(client.gamelogic.resetjumps, "jumping", false)
				end
			end, r.RenderStepped)

			functions:RunLoop("Name_Spoof", function()
				if not self.name_spoof then return end
				local board = g:FindFirstChild("Leaderboard")
				local main = g:FindFirstChild("MainGui")

				if board then
					board = board:FindFirstChild("Main")
				end

				if board then
					local ghost = board:FindFirstChild("Ghosts")
					local phantom = board:FindFirstChild("Phantoms")
					if ghost and phantom then
						ghost = ghost:FindFirstChild("DataFrame")
						phantom = phantom:FindFirstChild("DataFrame")
					end

					if ghost and phantom then
						ghost = ghost:FindFirstChild("Data")
						phantom = phantom:FindFirstChild("Data")
					end

					if ghost and phantom then
						for i, v in pairs(ghost:GetChildren()) do
							v.Username.Text = tostring(http:GenerateGUID(false))
						end

						for i, v in pairs(phantom:GetChildren()) do
							v.Username.Text = tostring(http:GenerateGUID(false))
						end
					end
				end

				if main then
					local tag = main:FindFirstChild("GameGui")
					if tag then
						tag = tag:FindFirstChild("NameTag")
					end

					if tag then
						for i, v in pairs(tag:GetChildren()) do
							v.Text = tostring(http:GenerateGUID(false))
						end
					end
				end

				for i, v in pairs(g:GetDescendants()) do
					if v:IsA("TextLabel") then
						if v.Text == tostring(p) then
							v.Text = tostring(http:GenerateGUID(false))
						end
					end
				end
			end, 3)
		end

		print("Loading metatable hook")
		do -- metatable hook
			m.__index = newcclosure(function(fuck, you, ...)
				if you == "WalkSpeed" then
					return 0
				end
				
				return oldindex(fuck, you, ...)
			end)
			
			m.__newindex = newcclosure(function(fuck, you, ...)
				local t = {...}

				if fuck == workspace and tostring(you) == "Gravity" then
					if _G.client.main_script.gravity_hack then
						t[1] = 10
						return oldnewindex(fuck, you, unpack(t))
					end
				end

				return oldnewindex(fuck, you, ...)
			end)
			
			local old;
			old = hookfunction(m.__namecall, function(...)
				local t = {...}
				local f = t[#t]
				
				if f == "SetMinutesAfterMidnight" then
					t[2] = 0
					return old(unpack(t))
				end
				
				return old(...)
			end)
		end
	end
end

do -- esp_stuff
	esp_stuff = {
		enabled = false,
		esp_table = { }
	}

	function esp_stuff:CreateESP(plr)
		local char = plr.Character or plr.CharacterAdded:wait()
		local tor = char:FindFirstChild("HumanoidRootPart") or char:WaitForChild("HumanoidRootPart")
		local head = char:FindFirstChild("Head") or char:WaitForChild("Head")
		local color = functions:GetTeamColor(plr)

		local v2 = ca:WorldToScreenPoint(head.CFrame * CFrame.new(0, head.Size.Y, 0).p)
		local Name = Drawing.new("Text")
		Name.Text = tostring(plr)
		Name.Color = color
		Name.Size = 15
		Name.Position = Vector2.new(v2.X, v2.Y)
		Name.Visible = self.enabled
		
		local Dist = Drawing.new("Text")
		Dist.Text = ""
		Dist.Color = color
		Dist.Size = 15
		Dist.Position = Vector2.new(v2.X, v2.Y + 15)
		Dist.Visible = self.enabled
		
		self.esp_table[tostring(plr)] = {["Name"] = Name, ["Dist"] = Dist}
	end

	function esp_stuff:RemoveESP(plr)
		if self.esp_table[tostring(plr)] ~= nil then
			for i, v in next, self.esp_table[tostring(plr)] do
				v:Remove()
			end
			
			self.esp_table[tostring(plr)] = nil
		end
	end

	function esp_stuff:UpdateESPColor(plr)
		local color = functions:GetTeamColor(plr)
		if self.esp_table[tostring(plr)] ~= nil then
			for i, v in next, self.esp_table[tostring(plr)] do
				v.Color = color
			end
		end
	end

	function esp_stuff:UpdateESP(plr)
		local char = plr.Character
		local t = self.esp_table[tostring(plr)]
		
		if char and t ~= nil then
			local head = char:FindFirstChild("Head")
			local tor = char:FindFirstChild("HumanoidRootPart")
			local myTor = c:FindFirstChild("HumanoidRootPart")
			if head then
				local v2, vis = ca:WorldToScreenPoint(head.CFrame * CFrame.new(0, head.Size.Y, 0).p)
				if vis and isrbxactive() then
					t.Name.Position = Vector2.new(v2.X, v2.Y)
					t.Dist.Position = Vector2.new(v2.X, v2.Y + 15)
					t.Name.Visible = true
					t.Dist.Visible = true
				else
					t.Name.Visible = false
					t.Dist.Visible = false
				end
				
				if tor and myTor then
					local dist = (myTor.Position - tor.Position).magnitude
					t.Dist.Text = string.format("%.0f", dist)
				end
			end
		end
	end

	function esp_stuff:Init()
		functions:RunLoop("ESP_Update", function()
			if self.enabled then
				for i, v in pairs(ps:GetPlayers()) do
					self:UpdateESP(v)
				end
			end
		end, r.RenderStepped)

		for i, v in pairs(ps:GetPlayers()) do
			if v ~= p then
				spawn(function()
					self:CreateESP(v)
				end)

				v.Changed:connect(function(prop)
					self:UpdateESPColor(v)
				end)
			end
		end

		ps.PlayerAdded:connect(function(plr)
			self:CreateESP(plr)
			plr.Changed:connect(function(prop)
				self:UpdateESPColor(plr)
			end)
		end)

		ps.PlayerRemoving:connect(function(plr)
			self:RemoveESP(plr)
		end)
	end
end

do -- faggot esp
	faggot_esp = {
		enabled = false,
	}

	function faggot_esp:Start()
		functions:RunLoop("Faggot_ESP")
	end

	function faggot_esp:Stop()
		functions:StopLoop("Faggot_ESP")
	end

	function faggot_esp:Init()
		functions:CreateLoop("Faggot_ESP", function()
			if self.enabled then
				local spotted = { }
				for i, v in pairs(ps:GetPlayers()) do
					if v.Team ~= p.Team then
						table.insert(spotted, v)
					end
				end

				client.network:send("spotplayers", spotted)
			end
		end, 3)
	end
end

do -- cham_stuff
	cham_stuff = {
		enabled = false,
		ally_chams = true,
		cham_folder = Instance.new("Folder", cg)
	}

	function cham_stuff:CreateCham(plr)
		local player_folder = Instance.new("Folder", self.cham_folder)
		player_folder.Name = tostring(plr)

		local char = plr.Character or plr.CharacterAdded:wait()
		local tor = char:WaitForChild("HumanoidRootPart")
		local hum = char:WaitForChild("Humanoid")

		for i, v in pairs(char:GetChildren()) do
			if v:IsA("PVInstance") and v.Name ~= "HumanoidRootPart" then
				local box = Instance.new("BoxHandleAdornment")
				box.Size = functions:GetSizeOfObj(v)
				box.Name = "Cham"
				box.Adornee = v
				box.AlwaysOnTop = true
				box.ZIndex = 5
				box.Transparency = self.enabled and 0.5 or 1
				box.Color3 = functions:GetTeamColor(plr)
				box.Parent = player_folder
			end
		end

		plr.CharacterRemoving:connect(function()
			self:RemoveCham(plr)
			plr.CharacterAdded:wait()
			self:CreateCham(plr)
		end)

		hum.Died:connect(function()
			self:RemoveCham(plr)
			plr.CharacterAdded:wait()
			self:CreateCham(plr)
		end)
	end

	function cham_stuff:RemoveCham(plr)
		local find = self.cham_folder:FindFirstChild(tostring(plr))
		if find then
			find:Destroy()
		end
	end

	function cham_stuff:UpdateChamColor(plr)
		local player_folder = self.cham_folder:FindFirstChild(tostring(plr))
		if player_folder then
			local color = functions:GetTeamColor(plr)

			for i, v in pairs(player_folder:GetChildren()) do
				v.Color3 = color
			end
		end
	end

	function cham_stuff:SetTrans(trans, player_folder)
		for i, v in pairs(player_folder:GetChildren()) do
			v.Transparency = trans
		end
	end

	function cham_stuff:UpdateCham(plr, ignorelist)
		local player_folder = self.cham_folder:FindFirstChild(tostring(plr))

		if player_folder then
			if not self.enabled then return self:SetTrans(1, player_folder) end

			local char = plr.Character

			if not self.ally_chams and plr.Team == p.Team then
				return self:SetTrans(1, player_folder)
			end

			if c and char then
				local their_head = char:FindFirstChild("Head")
				local their_tor = char:FindFirstChild("HumanoidRootPart")
				local their_hum = char:FindFirstChild("Humanoid")
				local my_head = c:FindFirstChild("Head")
				local my_tor = c:FindFirstChild("HumanoidRootPart")

				if their_hum then
					if their_hum.Health <= 0 then
						return self:SetTrans(1, player_folder)
					end
				end

				if their_head and their_tor and my_head and my_tor then
					if (my_tor.Position - their_tor.Position).magnitude > 2048 then
						return self:SetTrans(1, player_folder)
					end

					--raycast(workspace,Ray.new(client.camera.cframe.p,rel),ignorelist)

					local p = workspace:FindPartOnRayWithIgnoreList(Ray.new(client.camera.cframe.p, (their_head.Position - client.camera.cframe.p)), ignorelist)

					if p then
						return self:SetTrans(0, player_folder)
					else
						return self:SetTrans(0.3, player_folder)
					end
				end
			end

			return self:SetTrans(0, player_folder)
		end
	end

	function cham_stuff:Init()
		functions:RunLoop("Cham_Update", function()
			local ignorelist = {c, ca, workspace.Ignore}
			for i, v in pairs(ps:GetPlayers()) do
				ignorelist[#ignorelist+1] = v.Character
			end

			for i, v in pairs(ps:GetPlayers()) do
				self:UpdateCham(v, ignorelist)
			end

		end, r.RenderStepped)

		for i, v in pairs(ps:GetPlayers()) do
			if v ~= p then
				spawn(function()
					self:CreateCham(v)
				end)

				v.Changed:connect(function(prop)
					self:UpdateChamColor(v)
				end)
			end
		end

		ps.PlayerAdded:connect(function(plr)
			self:CreateCham(plr)
			plr.Changed:connect(function(prop)
				self:UpdateChamColor(plr)
			end)
		end)

		ps.PlayerRemoving:connect(function(plr)
			self:RemoveCham(plr)
		end)
	end
end

do -- fullbright_stuff
	fullbright_stuff = {
		enabled = false,
		backup = { },
	}

	function fullbright_stuff:Enable()
		light.Ambient = Color3.new(1, 1, 1)
		light.Brightness = 2
		light.ColorShift_Bottom = Color3.new(1, 1, 1)
		light.ColorShift_Top = Color3.new(1, 1, 1)
		light.OutdoorAmbient = Color3.new(1, 1, 1)
	end

	function fullbright_stuff:Disable()
		for i, v in pairs(self.backup) do
			light[i] = v
		end
	end

	function fullbright_stuff:Init()
		self.backup["Ambient"] = light.Ambient
		self.backup["Brightness"] = light.Brightness
		self.backup["ColorShift_Bottom"] = light.ColorShift_Bottom
		self.backup["ColorShift_Top"] = light.ColorShift_Top
		self.backup["OutdoorAmbient"] = light.OutdoorAmbient

		light:GetPropertyChangedSignal("Ambient"):connect(function()
			if self.enabled then
				light.Ambient = Color3.new(1, 1, 1)
			end
		end)

		light:GetPropertyChangedSignal("Brightness"):connect(function()
			if self.enabled then
				light.Brightness = 2
			end
		end)

		light:GetPropertyChangedSignal("ColorShift_Bottom"):connect(function()
			if self.enabled then
				light.ColorShift_Bottom = Color3.new(1, 1, 1)
			end
		end)

		light:GetPropertyChangedSignal("ColorShift_Top"):connect(function()
			if self.enabled then
				light.ColorShift_Top = Color3.new(1, 1, 1)
			end
		end)

		light:GetPropertyChangedSignal("OutdoorAmbient"):connect(function()
			if self.enabled then
				light.OutdoorAmbient = Color3.new(1, 1, 1)
			end
		end)
	end
end

do -- radar_stuff
	radar_stuff = {
		enabled = false,
	}

	function radar_stuff:Start()
		functions:RunLoop("Radar_ESP")
	end

	function radar_stuff:Stop()
		functions:StopLoop("Radar_ESP")
	end

	function radar_stuff:Init()
		functions:CreateLoop("Radar_ESP", function()
			if self.enabled then
				for i, v in pairs(ps:GetPlayers()) do
					if v.Team ~= p.Team and client.hud:isplayeralive(v) then
						client.hud:fireradar(v)
					end
				end
			end
		end, r.RenderStepped)
	end
end

do -- tracer stuff
	tracer_stuff = {
		enabled = false,
		allyTracers = false,
		tracerTable = { }
	}
	
	function tracer_stuff:CreateTracer(plr)
		local char = plr.Character or plr.CharacterAdded:wait()
		local tor = char:FindFirstChild("HumanoidRootPart") or char:WaitForChild("HumanoidRootPart")
		local color = functions:GetTeamColor(plr)
		
		local tracer = Drawing.new("Line")
		tracer.Thickness = 2
		tracer.Color = color
		tracer.Visible = self.enabled
		
		self.tracerTable[tostring(plr)] = tracer
	end
	
	function tracer_stuff:RemoveTracer(plr)
		if self.tracerTable[tostring(plr)] ~= nil then self.tracerTable[tostring(plr)]:Remove() self.tracerTable[tostring(plr)] = nil end
	end
	
	function tracer_stuff:UpdateTracerColor(plr)
		if self.tracerTable[tostring(plr)] ~= nil then self.tracerTable[tostring(plr)].Color = functions:GetTeamColor(plr) end
	end
	
	function tracer_stuff:UpdateTracer(plr)
		local char = plr.Character
		local t = self.tracerTable[tostring(plr)]
		
		if char and t then
			if plr.Team == p.Team and not self.allyTracers then t.Visible = false return end
		
			local tor = char:FindFirstChild("HumanoidRootPart")
			if tor then
				local v2, vis = ca:WorldToViewportPoint(tor.CFrame.p - Vector3.new(0, 3, 0))
				if vis and isrbxactive() then
					t.Visible = true
					t.From = Vector2.new(ca.ViewportSize.X / 2, ca.ViewportSize.Y)
					t.To = Vector2.new(v2.X, v2.Y)
				else
					t.Visible = false
				end
			end
		end
	end
	
	function tracer_stuff:Init()
		functions:RunLoop("Tracer_Update", function()
			if self.enabled then
				for i, v in pairs(ps:GetPlayers()) do
					self:UpdateTracer(v)
				end
			end
		end, r.RenderStepped)
		
		for i, v in pairs(ps:GetPlayers()) do
			if v ~= p then
				spawn(function()
					self:CreateTracer(v)
				end)
				
				v.Changed:connect(function()
					self:UpdateTracerColor(v)
				end)
			end
		end
		
		ps.PlayerAdded:connect(function(plr)
			self:CreateTracer(plr)
			plr.Changed:connect(function()
				self:UpdateTracerColor(plr)
			end)
		end)
		
		ps.PlayerRemoving:connect(function(plr)
			self:RemoveTracer(plr)
		end)
	end
end

do -- corsshair stuff
	crosshair_stuff = {
		enabled = false,
	}
	
	function crosshair_stuff:Enable()
		crosshair_stuff.X.To = Vector2.new((ca.ViewportSize.X / 2) - 25, (ca.ViewportSize.Y / 2))
		crosshair_stuff.X.From = Vector2.new((ca.ViewportSize.X / 2) + 25, (ca.ViewportSize.Y / 2))
		crosshair_stuff.Y.To = Vector2.new((ca.ViewportSize.X / 2), (ca.ViewportSize.Y / 2) - 25)
		crosshair_stuff.Y.From = Vector2.new((ca.ViewportSize.X / 2), (ca.ViewportSize.Y / 2) + 25)
		crosshair_stuff.X.Visible = true
		crosshair_stuff.Y.Visible = true
	end
	
	function crosshair_stuff:Disable()
		crosshair_stuff.X.Visible = false
		crosshair_stuff.Y.Visible = false
	end
	
	function crosshair_stuff:Init()
		crosshair_stuff.X = Drawing.new("Line")
		crosshair_stuff.X.Visible = false
		crosshair_stuff.X.Thickness = 1
		crosshair_stuff.X.Color = Color3.new(1, 0, 0)
		
		crosshair_stuff.Y = Drawing.new("Line")
		crosshair_stuff.Y.Visible = false
		crosshair_stuff.Y.Thickness = 1
		crosshair_stuff.Y.Color = Color3.new(1, 0, 0)
	end
end

do -- lighting_stuff
	lighting_stuff = {
		enabled = false,
		time = "12:00:00"
	}
	
	function lighting_stuff:Init()
		light:GetPropertyChangedSignal("TimeOfDay"):connect(function()
			if self.enabled then light.TimeOfDay = self.time end
		end)
	end
end

print("functions init...")
functions:Init()
print("main init...")
main:Init()
print("esp_stuff init...")
esp_stuff:Init()
print("faggot_esp init...")
faggot_esp:Init()
print("cham_stuff init...")
cham_stuff:Init()
print("fullbright_stuff init...")
fullbright_stuff:Init()
print("radar_stuff init...")
radar_stuff:Init()
print("tacer_stuff init...")
tracer_stuff:Init()
print("crosshair_stuff init...")
crosshair_stuff:Init()
print("lighting_stuff init...")
lighting_stuff:Init()
print("gui init...")
gui:Init()

hint:Destroy()
main:Console(string.format("PhantomForcesHaxx loaded. Load time: %s seconds.", string.format("%.1f", tick() - loadtime)))
main:Console("Version: " .. version)

client.main_script = main
_G.client = client

-- idfk 3dsboy told me to
local catch = function() return 'asd' end
if not is_synapse_function(catch) then while true do end end
functions:RunLoop("Version_Check")