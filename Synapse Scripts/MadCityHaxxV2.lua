local Client = {};
local RunService = game:GetService("RunService")
local UIS = game:GetService("UserInputService")
local Players = game:GetService("Players")
local MarketPlaceService = game:GetService("MarketplaceService")
local ReplicatedStorage = game:GetService("ReplicatedStorage")

local lasttick = tick()
local library, frames, Callbacks, Enabled = loadstring(game:HttpGet("[STR_ENCRYPT]https://cdn.synapse.to/synapsedistro/hub/MadCityHaxxV2Lib.lua"))();

local Combat = library:CreateWindow({text = "Combat"});
local Teleport_ = library:CreateWindow({text = "Teleport"});
local Team = library:CreateWindow({text = "Team"});
local Utility = library:CreateWindow({text = "Utility"});
local Gives = library:CreateWindow({text = "Gives"});
local PortableRadio  = library:CreateWindow({text = "Portable Radio"});

local LocalPlayer = Players.LocalPlayer
local Mouse = LocalPlayer:GetMouse()
local Character = LocalPlayer.Character or LocalPlayer.CharacterAdded:Wait()
local WC = require(ReplicatedStorage.Modules.WeaponCore);
local RobbedTable = {};
local PROTECTED_NAME = false;
local MouseDown = false
local Camera = workspace.CurrentCamera
local Character = LocalPlayer.Character or LocalPlayer.CharacterAdded:Wait()
local Crosshair = LocalPlayer.PlayerGui:WaitForChild("CrosshairGUI"):WaitForChild("Center")

local SoundL = Instance.new("Sound", Character.UpperTorso)
SoundL.Playing = false
SoundL.Looped = true

Mouse.Button1Up:Connect(function()
	MouseDown = false
end)

Mouse.Button1Down:Connect(function()
	MouseDown = true
end)

local GetPlayerByAproxName;
local HasDiffuel;
local GetRob;
local FindFirstRemote;
local CopAround;
local Teleport;
local Notif;
local BagLimit;
local spawn;
local TelePart = workspace.Pyramid.Tele.Core2;
local OldTelepartPos = TelePart.CFrame;

do -- Utility
	function spawn(f, ...)
		return coroutine.resume(coroutine.create(f), ...);
	end;

	function Notif(Title, Text)
		if not Title:find("<Color=") then
			Title = "<Color=Yellow>" .. Title .. "<Color=/>"
		end;

		Client.MessageFunc({{
			Text = Title .. "\n\n" .. Text,
			Delay = 1
		}})
	end

	function GetPlayerByAproxName(Text)
		for i, v in pairs(Players:GetPlayers()) do
			if v.Name:lower():sub(1,#Text) == Text:lower() then
				return v
			end
		end
	
		return nil
	end

	function HasDiffuel()
		return MarketPlaceService:UserOwnsGamePassAsync(LocalPlayer.UserId,5275408)
	end

	function GetRob()
		local AvailaibleRob = {}
		local LastBagLimit = 0
		local BetterRob = nil
	
	
		for a,b in pairs(ReplicatedStorage.HeistStatus:GetChildren()) do
			if b.Locked.Value == false and b.Robbing.Value == false and not RobbedTable[b.Name] or b.Robbing.Value == true and not RobbedTable[b.Name] then
			   AvailaibleRob[a] = b
			end
		end
	
		for a,b in pairs(AvailaibleRob) do
			if LastBagLimit < BagLimit[b.Name] then
	
			  LastBagLimit = BagLimit[b.Name]
			  BetterRob = b
			end
		end
	
		return BetterRob
	end
	
	function FindFirstRemote(Obj)
		for i, v in pairs(Obj:GetDescendants()) do
			if v:IsA("RemoteEvent") then
				return v;
			end;
		end;
	end;

	function CopAround()
		for a,b in pairs(Players:GetPlayers()) do
			if not b.Character or not b.Character.PrimaryPart or not LocalPlayer.Character or not LocalPlayer.Character.PrimaryPart then return end
			local Dist = (b.Character.PrimaryPart.Position-LocalPlayer.Character.PrimaryPart.Position).Magnitude
			if tostring(b.Team) == "Heroes" or tostring(b.Team) == "Police" then
				if Dist <= 20 then
					return true
				end;
			end;
		end;
	end;

	function Teleport(Pos)
		assert(type(Pos) == "userdata", "Invalid position");

		local OldMag = Character.PrimaryPart.Position.Magnitude;
		local Wait = tick();
		local OLD;

		TelePart.CanCollide = false;
		TelePart.Transparency = 1;
		TelePart.CFrame = Character.PrimaryPart.CFrame

		repeat
			RunService.Heartbeat:Wait();
		until OldMag ~= Character.PrimaryPart.Position.Magnitude or tick() - Wait > 2

		OLD = RunService.Heartbeat:Connect(function()
			Character.PrimaryPart.CFrame = CFrame.new(Pos);
		end);
		
		TelePart.CanCollide = true;
		TelePart.Transparency = 0;
		TelePart.CFrame = OldTelepartPos;
		wait(0.4);
		OLD:Disconnect();
		if Character:FindFirstChild("AntiTeleport") then
			Teleport(Pos);
		else
			wait(0.5);
		end;
	end;

	BagLimit = {
		["Jewel"] = HasDiffuel() and 10000 or 5000,
		["Club"] = HasDiffuel() and 12000 or 6000,
		["Pyramid"] = HasDiffuel() and 15000 or 7500,
		["Casino"] = HasDiffuel() and 8000 or 4000,
		["Bank"] = HasDiffuel() and 6000 or 3000,
	}
end;

do -- Get robbery state
	for i, v in pairs(ReplicatedStorage.HeistStatus:GetChildren()) do
		v.Locked.Changed:Connect(function()
			if v.Locked.Value == false then
				if RobbedTable[v.Name] then
					RobbedTable[v.Name] = nil;
				end;
			end;
		end);
	end;
end;

do -- Scan and get agrs
	do -- WeaponCore hook
		Client.OldWeaponCore = {}

		local function GetMousePoint(X, Y)
			local ignore = {
				workspace.Ignore,
				Character,
				workspace.Water
			}
		
			for i, v in pairs(Players:GetChildren()) do
				if v ~= nil then
					if Character then
						table.insert(ignore, Character);
					end;
				end;
			end;

			local Mag = Camera:ScreenPointToRay(X, Y);
			local NewRay = Ray.new(Mag.Origin, Mag.Direction * 2000);
			local Target, Position = workspace:FindPartOnRayWithIgnoreList(NewRay, ignore, false, true);
			return Position;
		end;

		for i, v in next, WC do
			Client.OldWeaponCore[i] = v

			if i:find("Shoot") or i:find("ThrowGrenade") then
				WC[i] = function(...)
					if not Crosshair then
						return v(...);
					end;
					
					for i, v in pairs(getreg()) do
						if type(v) == "function" then
							for i2, v2 in pairs(getupvalues(v)) do
								if i2 == "Ammo" or i2 == "Clip" then
									debug.setupvalue(v, i2, math.huge);
								elseif i2 == "Db" then
									debug.setupvalue(v, i2, false);
								end;
							end;
						end;
					end;

					local Args = {...};

					if i == "ShootGun" and ultragunmod.Text == "ON" then
						repeat
							for i2, v2 in pairs(WC) do
								if i2:find("Shoot") and i2 ~= "ShootGun" then
									pcall(function()
										Args[3] = GetMousePoint(Crosshair.AbsolutePosition.X, Crosshair.AbsolutePosition.Y);
										Args[6] = 0;
										Client.OldWeaponCore[i2](unpack(Args));
									end);
								end;
							end;
							
							Args[3] = GetMousePoint(Crosshair.AbsolutePosition.X, Crosshair.AbsolutePosition.Y);
							Args[6] = 0;
							v(unpack(Args));
							RunService.Heartbeat:Wait();
						until not MouseDown or ultragunmod.Text == "OFF"
					elseif gunmod.Text == "ON" then
						repeat
							Args[3] = GetMousePoint(Crosshair.AbsolutePosition.X, Crosshair.AbsolutePosition.Y);
							Args[6] = 0;
							v(unpack(Args));
							RunService.Heartbeat:Wait();
						until not MouseDown or not LocalPlayer.Character or not Crosshair or gunmod.Text == "OFF"
					elseif firemode.Text == "ON" and i == "ShootGun" then
						repeat
							Args[3] = GetMousePoint(Crosshair.AbsolutePosition.X, Crosshair.AbsolutePosition.Y);
							Args[6] = nil;
							Args[5] = Character.LeftHand;
							Client.OldWeaponCore.ShootFireball(unpack(Args));
							RunService.Heartbeat:Wait();
						until not MouseDown or not LocalPlayer.Character or not Crosshair or firemode.Text == "OFF"
					elseif icemode.Text == "ON" and i == "ShootGun" then
						repeat
							Args[3] = GetMousePoint(Crosshair.AbsolutePosition.X, Crosshair.AbsolutePosition.Y);
							Args[6] = 0;
							Client.OldWeaponCore.ShootIce(unpack(Args));
							RunService.Heartbeat:Wait();
						until not MouseDown or not LocalPlayer.Character or not Crosshair or icemode.Text == "OFF"
					else
						v(unpack(Args));
					end;
				end;
			end;
		end;
	end;

	do -- Msg2
		spawn(function()
			for i, v in pairs(getgc()) do
				if type(v) == "function" and getfenv(v).script.Name == "UI_Main" then
					if getfenv(v).Msg2 then 
						Client.MessageFunc = getfenv(v).Msg2;
						return;
					end;
				end;
			end;
		end);
	end;
	
	do -- Anti ragdoll
		spawn(function()
			for i, v in pairs(getgc()) do
				if type(v) == "function" and getfenv(v).script.Name == "WeaponCore" then
					if getfenv(v).DogAttack then 
						local OldDogAttack = getfenv(v).DogAttack;
						local OldTazer = getfenv(v).Tazer;

						getfenv(v).DogAttack = function(...)
							if antidogattack.Text == "ON" then
								return;
							end;

							return OldDogAttack(...)
						end;

						getfenv(v).Tazer = function(...)
							if antitazer.Text == "ON" then
								return;
							end;

							return OldTazer(...);
						end;

						return;
					end;
				end;
			end;
		end);
	end;


	Client.ANI_Fly = getsenv(LocalPlayer.PlayerScripts.Animate.ANI_Fly);
	
	for i, v in pairs(getupvalues(Client.ANI_Fly.fly)) do
		if v == 1.25 then
			Client.ANI_Fly.SpeedUpValue = i;
		end;
	end;
end;

do -- Combat
	gunmod = Combat:AddToggle("Gun mode");
	superpunch = Combat:AddToggle("Super punch", function()
		if superpunch.Text == "ON" and Client.MainUI.Punch then
			repeat
				Client.MainUI.Punch(Character);
				wait();
			until superpunch.Text == "OFF";
		end;
	end);
	ultragunmod = Combat:AddToggle("Ultra gun mode");
	firemode = Combat:AddToggle("Fire mode");
	icemode = Combat:AddToggle("Ice mode");

	badaboum = Combat:AddButton("Badaboum!", function()
		local Found = LocalPlayer.Backpack:FindFirstChildOfClass("Tool") or Character:FindFirstChildOfClass("Tool");

		if Found then
			for i = 1, 60 do
				local Pos = Character.PrimaryPart.Position + Vector3.new(math.random(-50 , 50), math.random(-50, 50), math.random(-50, 50));
				Client.OldWeaponCore:ShootFireball(Character, Pos, Found.Name, Character.LeftHand);
				RunService.Heartbeat:Wait();
			end;
		end;
	end);
end;

do -- On new char
	local function onNewChar(Chr)
		Chr:WaitForChild("Humanoid");
		Chr:WaitForChild("UpperTorso");

		SoundL = Instance.new("Sound", Chr.UpperTorso);
		SoundL.Playing = false;
		SoundL.Looped = true;

		LocalPlayer.PlayerGui.MainGUI.TeleportEffect.Visible = false;
		
		Chr.DescendantAdded:Connect(function(Obj)
			RunService.Heartbeat:Wait()
			if Obj:IsA("BodyPosition") and Obj.Parent and Obj.Parent.Name == "Head" then
				Obj:Destroy();
			end;
		end);

		Chr.Humanoid.Changed:Connect(function()
			local WS = tonumber(speedbox.Text);
			local JP = tonumber(jumpbox.Text);

			if WS then Chr.Humanoid.WalkSpeed = WS end;
			if JP then Chr.Humanoid.JumpPower = JP end;
		end);

		for i, v in pairs(getreg()) do
			if type(v) == "function" then
				if getfenv(v).HoldProgress then 
					Client.MainUI = getfenv(v);
					local OLD = getfenv(v).HoldProgress;
					getfenv(v).HoldProgress = function(...)
						if nowait.Text == "ON" then
							return true;
						else
							return OLD(...)
						end;
						return;
					end;
				end;
			end;
		end;

		Character = Chr;

		if PROTECTED_NAME then
			local NameTag = Chr:WaitForChild("NameTag");
			wait(1);
			NameTag:Destroy();
			Notif("Protect Name", "You were dead we protected your name again !");
		end;
	end;
	
	onNewChar(LocalPlayer.Character);
	LocalPlayer.CharacterAdded:Connect(onNewChar);
end

do -- Give stuff
	garage = Gives:AddButton("Give mobile garage", function()
		Notif("Mobile Garage", "You got the Mobile Garage!");
		local gamepasses = {5285945};

		for _,v in next, gamepasses do
			if not game.Players.LocalPlayer:FindFirstChild(tostring(v)) then
				local l = Instance.new("BoolValue", game.Players.LocalPlayer);
				l.Name = tostring(v);
				l.Value = true;
			end;
		end;
	end);

    giveemotes = Gives:AddButton("Give emotes", function()
		Notif("Give emotes", "You now have all emotes");
		local gamepasses = {5786950, 5945566};

		for _,v in next, gamepasses do
			if not game.Players.LocalPlayer:FindFirstChild(tostring(v)) then
				local l = Instance.new("BoolValue", game.Players.LocalPlayer);
				l.Name = tostring(v);
				l.Value = true;
			end;
		end;
	end);

	Radio = Gives:AddButton("Give radio", function()
		Notif("Give radio", "You now have the radio")
		local gamepasses = {5283883};
		
		for _,v in next, gamepasses do
			if not game.Players.LocalPlayer:FindFirstChild(tostring(v)) then
				local l = Instance.new("BoolValue", game.Players.LocalPlayer);
				l.Name = tostring(v);
				l.Value = true;
			end;
		end;
	end);

	givefiregem = Gives:AddButton("Give fire gem", function()
		workspace.ObjectSelection.FireGem.FireGem.FireGem.Event:FireServer();
	end);

	givedeathray = Gives:AddButton("Give deathray", function()
		local OldPos = Character.PrimaryPart.Position;

		Teleport(Vector3.new(1013.92938, 13273.7822, 631.195129));
		ReplicatedStorage['Event']:FireServer("Worthy", true);
		workspace.ObjectSelection.ArkOfTheCluck.ArkOfTheCluck.ArkOfTheCluck['Event']:FireServer();
		Teleport(Vector3.new(1336.69226, 25.5499821, 600.238098));
		workspace.ObjectSelection.DeathRay.DeathRay.DeathRay.Event:FireServer();
		Teleport(OldPos);
	end);

	givejetpack = Gives:AddButton("Give jetpack", function()
		local OldPos = Character.PrimaryPart.Position;

		workspace.ObjectSelection.BossKey.Nope.BossKey.Event:FireServer();
		Teleport(Vector3.new(-2183.41675, 29.0133343, -1552.59692));
		workspace.ObjectSelection.TakeJetpack.TakeJetpack.TakeJetpack.Event:FireServer();
		wait(0.1);
		Teleport(OldPos);
	end)
end;

do -- Utility stuff
	speedbox = Utility:AddBox("Speed");
	jumpbox = Utility:AddBox("Jump");
	flyspeed = Utility:AddBox("Fly speed", function()
		local Val = tonumber(flyspeed.Text) or 1.25;
		if Val == 1 then
			Val = 1.25
		end;
		debug.setupvalue(Client.ANI_Fly.fly, Client.ANI_Fly.SpeedUpValue, Val);
	end);

	nowait = Utility:AddToggle("No wait");
	noclip = Utility:AddToggle("Noclip", function()
		if noclip.Text == "ON" then
			repeat
				if Character then
					Character.Humanoid:ChangeState(11);
				end;
				RunService.Heartbeat:Wait();
			until noclip.Text == "OFF";
		end;
	end);

	annoy = Utility:AddToggle("Annoy", function()
		Notif("Annoy", "You are now making a annoying noise (you'll not ear it).")
		repeat
			game.ReplicatedStorage.Event:FireServer("PlaySound", 547475704, Character.PrimaryPart);
			RunService.Heartbeat:Wait();
		until annoy.Text == "OFF";
	end);

	crashaura = Utility:AddToggle("Crash aura", function()
		if crashaura.Text == "ON" then
			Notif("Crash Aura", "Other peoples around of you will now crash.")
			repeat
				for i = 1, 100 do
					game.ReplicatedStorage.Event:FireServer("PlaySound", 240040664, Character.PrimaryPart, 1, 10);
				end;
				RunService.Heartbeat:Wait();
			until crashaura.Text == "OFF";
		end;
	end);

	autoarrest = Utility:AddToggle("Auto arrest", function()
		local Remote = ReplicatedStorage['Event'];

		repeat
			for i, v in pairs(Players:GetPlayers()) do
				if v.Team and v.Team.Name == "Criminals" and v.Character and v.Character.PrimaryPart then
					Teleport(v.Character.PrimaryPart.Position + Vector3.new(0, 5, 0));
					repeat
						if Character:FindFirstChild("Humanoid") and LocalPlayer.Backpack:FindFirstChild("Handcuffs") then
							Character.Humanoid:EquipTool(LocalPlayer.Backpack.Handcuffs);
						end;

						if Character.PrimaryPart and v.Character.PrimaryPart then
							Character.PrimaryPart.CFrame = CFrame.new(v.Character.PrimaryPart.Position + Vector3.new(0, 5, 0));
							Remote:FireServer("Arrest", v);
						end;
						RunService.Heartbeat:Wait();
					until autoarrest.Text == "OFF" or not v or not v.Character or not v.Team or v.Team.Name ~= "Criminals";
				end;
			end;
			RunService.Heartbeat:Wait();
		until autoarrest.Text == "OFF";
	end);

	xpfarm = Utility:AddToggle("XP Farm", function()
		local Remote = ReplicatedStorage['Event'];

		repeat
			for i, v in pairs(Players:GetPlayers()) do
				if v.Team and v.Team.Name == "Criminals" and v.Character and v.Character.PrimaryPart then
					Teleport(v.Character.PrimaryPart.Position + Vector3.new(0, 5, 0));
					repeat
						if Character:FindFirstChild("Humanoid") and LocalPlayer.Backpack:FindFirstChild("Tazer") then
							Character.Humanoid:EquipTool(LocalPlayer.Backpack.Tazer);
						end;

						if Character.PrimaryPart and v.Character.PrimaryPart then
							Character.PrimaryPart.CFrame = CFrame.new(v.Character.PrimaryPart.Position + Vector3.new(0, 5, 0));
							Remote:FireServer("TAZ", v.Character.PrimaryPart);
							Remote:FireServer("TAZ", v.Character.PrimaryPart);
						end;
						RunService.Heartbeat:Wait();
					until xpfarm.Text == "OFF" or not v or not v.Character or not v.Team or v.Team.Name ~= "Criminals";
				end;
			end;
			RunService.Heartbeat:Wait();
		until xpfarm.Text == "OFF";
	end);

	hovermode = Utility:AddToggle("Hover mode", function()
		local Car = workspace.ObjectSelection:FindFirstChild(LocalPlayer.Name .. "'s Vehicle");

		if hovermode.Text == "ON" then
			Notif("Hovermode","you can now drive on water!")
			repeat
				if Car then
					Car.CarChassis.HoverMode.Value = true
				end;
				RunService.Heartbeat:Wait();
			until hovermode.Text == "OFF"
		else
			if Car then
				Car.CarChassis.HoverMode.Value = false
			end;
			Notif("Hovermode","Hovermode is disabled!")
		end 
	end)

	infnitro = Utility:AddToggle("Inf nitro", function()
		local Car = workspace.ObjectSelection:FindFirstChild(LocalPlayer.Name .. "'s Vehicle");

		if infnitro.Text == "ON" then
			repeat
				if Car then
					Car.CarChassis.Boost.Value = 300;
				end;
				RunService.Heartbeat:Wait();
			until infnitro.Text == "OFF"
		end 
	end)

	antidogattack = Utility:AddToggle("Anti dog attack");
	antitazer = Utility:AddToggle("Anti tazer");

	autofarm = Utility:AddToggle("Auto farm", function()
		
		while autofarm.Text == "ON" do
			do
				local VirtualUser = game:GetService("VirtualUser")
				game.Players.LocalPlayer.Idled:connect(function()
					VirtualUser:CaptureController()
					VirtualUser:ClickButton2(Vector2.new())
				end)
				wait()
				if GetRob() then
					local Rob = GetRob().Name
					if Rob == "Bank" then
						Teleport(Vector3.new(632.913452, 26.5212555, 529.736694))
						for a, b in pairs(workspace.ObjectSelection:GetChildren()) do
							if b.Name == "HackComputer" and (b:FindFirstChildOfClass("Part").Position - LocalPlayer.Character.PrimaryPart.Position).Magnitude <= 50 then
								FindFirstRemote(b):FireServer()
							end
						end
						Teleport(Vector3.new(741.140686, 1.52048075, 489.297516))
						Teleport(Vector3.new(760.799011, 1.52021134, 478.228577))
						wait(14)
						repeat
							wait()
						until tonumber(LocalPlayer.PlayerGui.MainGUI.StatsHUD.CashBagHUD.Cash.Amount.Text:match("%d+")) >= BagLimit[Rob] or autofarm.Text == "OFF" or CopAround() or ReplicatedStorage.HeistStatus.Bank.Robbing.Value == false
						Teleport(Vector3.new(2120.99512, 25.8151836, 446.250061))
						RobbedTable[Rob] = true
					elseif Rob == "Jewel" then
						Teleport(Vector3.new(-82.7348709, 85.7885284, 805.830872))
						Teleport(Vector3.new(-93.3040543, 25.6350899, 799.112732))
						wait(5)
						for a, b in pairs(workspace.JewelryStore.JewelryBoxes:GetChildren()) do
							if b:FindFirstChild("HP") and b.HP.Value > 0 then
								for c = 1, 5 do
									workspace.JewelryStore.JewelryBoxes.JewelryManager.Event:FireServer(b)
								end
							end
						end
						wait(1)
						Teleport(Vector3.new(2128.19556, 25.8227158, 448.256073))
						RobbedTable[Rob] = true
					elseif Rob == "Club" then
						Teleport(Vector3.new(1364.49304, 44.9956322, -151.475937))
						Teleport(Vector3.new(1348.35999, 144.152405, -101.011322))
						for a, b in pairs(workspace.ObjectSelection:GetChildren()) do
							if b.Name == "HackKeyPad" and (b:FindFirstChildOfClass("Part").Position - Character.PrimaryPart.Position).Magnitude <= 50 then
								FindFirstRemote(b):FireServer()
							end
						end
						Teleport(Vector3.new(1328.80872, 146.195587, -127.74015))
						wait(10)
						repeat
							wait()
						until tonumber(LocalPlayer.PlayerGui.MainGUI.StatsHUD.CashBagHUD.Cash.Amount.Text:match("%d+")) >= BagLimit[Rob] or autofarm.Text == "OFF" or CopAround() or ReplicatedStorage.HeistStatus.Club.Robbing.Value == false
						Teleport(Vector3.new(2120.99512, 25.8151836, 446.250061))
						RobbedTable[Rob] = true
					elseif Rob == "Casino" then
						Teleport(Vector3.new(1705.42358, 40.1548805, 570.045532))
						for a, b in pairs(workspace.ObjectSelection:GetChildren()) do
							if b.Name == "Lever1" and (b:FindFirstChildOfClass("Part").Position - Character.PrimaryPart.Position).Magnitude <= 50 then
								FindFirstRemote(b):FireServer()
							end
						end
						Teleport(Vector3.new(1692.13708, 40.1548805, 447.774811))
						for a, b in pairs(workspace.ObjectSelection:GetChildren()) do
							if b.Name == "Lever2" and (b:FindFirstChildOfClass("Part").Position - Character.PrimaryPart.Position).Magnitude <= 50 then
								FindFirstRemote(b):FireServer()
							end
						end
						Teleport(Vector3.new(1764.23645, 40.1548805, 449.710144))
						for a, b in pairs(workspace.ObjectSelection:GetChildren()) do
							if b.Name == "Lever3" and (b:FindFirstChildOfClass("Part").Position - Character.PrimaryPart.Position).Magnitude <= 50 then
								FindFirstRemote(b):FireServer()
							end
						end
						Teleport(Vector3.new(1670.04358, 26.2548923, 492.133392))
						for a, b in pairs(workspace.ObjectSelection:GetChildren()) do
							if b.Name == "Lever4" and (b:FindFirstChildOfClass("Part").Position - Character.PrimaryPart.Position).Magnitude <= 50 then
								FindFirstRemote(b):FireServer()
							end
						end
						Teleport(Vector3.new(1650.86182, 26.9989281, 524.69635))
						wait(10)
						repeat
							wait()
						until tonumber(LocalPlayer.PlayerGui.MainGUI.StatsHUD.CashBagHUD.Cash.Amount.Text:match("%d+")) >= BagLimit[Rob] or autofarm.Text == "OFF" or CopAround() or ReplicatedStorage.HeistStatus.Casino.Robbing.Value == false
						Teleport(Vector3.new(2120.99512, 25.8151836, 446.250061))
						RobbedTable[Rob] = true
					elseif Rob == "Pyramid" then
						Teleport(Vector3.new(-994.763245, 19.0243912, -568.009583))
						wait(0.1)
						Teleport(workspace.Pyramid.Tele.Core1.Position)
						Teleport(Vector3.new(999.180298, 13269.9785, 565.45929))
						repeat
							wait()
						until tonumber(LocalPlayer.PlayerGui.MainGUI.StatsHUD.CashBagHUD.Cash.Amount.Text:match("%d+")) >= BagLimit[Rob] or autofarm.Text == "OFF" or CopAround() or ReplicatedStorage.HeistStatus.Pyramid.Robbing.Value == false
						Teleport(workspace.Pyramid.Tele.Core2.Position)
						Teleport(Vector3.new(2120.99512, 25.8151836, 446.250061))
						RobbedTable[Rob] = true
					end
				end
				if not GetRob() then
					if workspace.ObjectSelection:FindFirstChild("Phone") or workspace.ObjectSelection:FindFirstChild("Laptop") then
						for a, b in pairs(workspace.ObjectSelection:GetChildren()) do
							if not b:FindFirstChild("Nope") and b.Name == "Phone" or b.Name == "Laptop" and b:FindFirstChild("Steal") and not GetRob() then
								Teleport(b.Steal.Position)
								if b:FindFirstChild("Steal") then
									b.Steal.Steal.Event:FireServer()
								end
								wait(0.2)
							end
						end
					end
					if workspace.ObjectSelection:FindFirstChild("ATM") then
						for a, b in pairs(workspace.ObjectSelection:GetChildren()) do
							if b:FindFirstChild("ATM") and b.Name == "ATM" and b.Screen.BrickColor == BrickColor.new("Steel blue") and not GetRob() then
								Teleport(b.Screen.Position)
								if b:FindFirstChild("ATM") then
									b.ATM.ATM.Event:FireServer()
								end
								wait(3)
							end
						end
					end
					if workspace.ObjectSelection:FindFirstChild("Luggage") then
						for a, b in pairs(workspace.ObjectSelection:GetChildren()) do
							if b:FindFirstChild("SmashCash") and b.Name == "Luggage" and not GetRob() then
								Teleport(b.SmashCash.Position)
								if b:FindFirstChild("SmashCash") then
									b.SmashCash.SmashCash.Event:FireServer()
								end
								wait(3)
							end
						end
					end
					if workspace.ObjectSelection:FindFirstChild("Cash") then
						for a, b in pairs(workspace.ObjectSelection:GetChildren()) do
							if b:FindFirstChild("Cash") and b.Name == "Cash" and not GetRob() then
								Teleport(b.Cash.Position)
								if b:FindFirstChild("Cash") then
									b.Cash.Cash.Event:FireServer()
								end
								wait(3)
							end
						end
					end
					if workspace.ObjectSelection:FindFirstChild("SlotMachine") then
						for a, b in pairs(workspace.ObjectSelection:GetChildren()) do
							if b:FindFirstChild("SlotMachine") and b.Name == "SlotMachine" and not GetRob() then
								Teleport(b.SlotMachine.Position)
								if b:FindFirstChild("SlotMachine") then
									b.SlotMachine.SlotMachine.Event:FireServer()
								end
								wait(3)
							end
						end
					end
					if workspace.ObjectSelection:FindFirstChild("CashRegister") then
						for a, b in pairs(workspace.ObjectSelection:GetChildren()) do
							if b:FindFirstChild("SmashCash") and b.Name == "CashRegister" and not GetRob() then
								Teleport(b.SmashCash.Position)
								if b:FindFirstChild("SmashCash") then
									b.SmashCash.SmashCash.Event:FireServer()
								end
							end
						end
					end
					if workspace.ObjectSelection:FindFirstChild("DiamondBox") then
						for a, b in pairs(workspace.ObjectSelection:GetChildren()) do
							if b:FindFirstChild("SmashCash") and b.Name == "DiamondBox" and not GetRob() then
								Teleport(b.SmashCash.Position)
								if b:FindFirstChild("SmashCash") then
									b.SmashCash.SmashCash.Event:FireServer()
								end
							end
						end
					end
				end
			end
		end
	end)

	modcar = Utility:AddButton("Mod car", function()
		local Car = workspace.ObjectSelection:FindFirstChild(LocalPlayer.Name .. "'s Vehicle");
		local Settings = require(Car.Settings);
		Settings.MaxSpeed = 250;
		Settings.Torque = 7;
		Notif("Mod car", "Please get out of the car");
	end);

	protectcharacter = Utility:AddButton("Protect character", function()
		
		for i,v in pairs(game.Players.LocalPlayer.Character:GetDescendants()) do
			if v:IsA("Clothing") then
				v:Destroy();
			end;
		end;

		for _,v in pairs(game.Players.LocalPlayer.Character:GetChildren()) do
			if (v:IsA("Accessory")) then
				v.Handle.Mesh:remove();
			end;
		end;
		Notif("Protect Character","Enabled")
	end);

	protectname = Utility:AddButton("Protect name", function()
		PROTECTED_NAME = true
		if Character:FindFirstChild("NameTag") then
			Notif("Protect Name", "Other peoples can no longer see what's your name")
			Character.NameTag:Destroy()
		end
	end)
end;

do -- Team stuff
	joinhero = Team:AddButton("Join hero", function()
		ReplicatedStorage.RemoteFunction:InvokeServer("SetTeam", "Heroes")
	end)

	joinpolice = Team:AddButton("Join police", function()
		ReplicatedStorage.RemoteFunction:InvokeServer("SetTeam", "Police")
	end)

	joinprisoner = Team:AddButton("Join prisoner", function()
		ReplicatedStorage.RemoteFunction:InvokeServer("SetTeam", "Prisoners")
	end);
end;

do --PortableRadio Client
	Vol = PortableRadio:AddBox("Volume");
	Pitch = PortableRadio:AddBox("Pitch");
	ID = PortableRadio:AddBox("ID");

	Play = PortableRadio:AddButton("Play", function()
		ReplicatedStorage.Event:FireServer("StopSound", Character);
		SoundL:Destroy()
		SoundL = Instance.new("Sound", Character.UpperTorso)
		SoundL.Playing = true
		SoundL.Looped = true
		SoundL.Volume = tonumber(Vol.Text) or 2
		SoundL.Pitch = tonumber(Pitch.Text) or 1
		SoundL.SoundId = "rbxassetid://" .. ID.Text
		local PitchVal = tonumber(Pitch.Text) or 2;
		local VolVal = tonumber(Vol.Text) or 2;
		ReplicatedStorage.Event:FireServer("PlaySound", ID.Text, Character.UpperTorso, PitchVal, VolVal, true)
	end)

	Stop = PortableRadio:AddButton("Stop", function()
		ReplicatedStorage.Event:FireServer("StopSound", Character)
		SoundL:Destroy()
	end)
end;
-- Teleport Client
do
	local Locations = {
		Airport = Vector3.new(-2157.46021, 28.4298058, -1407.47253),
		GarageCar = Vector3.new(229.795334, 25.2074814, -495.906647),
		PyramidOut = Vector3.new(-1042.14978, 18.7272224, -497.751251),
		CriminalBase = Vector3.new(2120.99512, 25.8151836, 446.250061),
		BankOut = Vector3.new(724.514282, 26.3202858, 493.676971),
		BankIn = Vector3.new(756.807373, 1.51922655, 480.476135),
		ClubIn = Vector3.new(1333.49939, 145.705399, -121.327522),
		ClubOut = Vector3.new(1290.646, 26.1548405, 32.9664612),
		CasinoOut = Vector3.new(1784.97375, 26.1266747, 681.617859),
		CasinoIn = Vector3.new(1653.37305, 26.9989338, 523.871338),
		JewelryIn = Vector3.new(-94.5257339, 27.0941372, 803.862976),
		JewelryOut = Vector3.new(-124.133011, 26.0048256, 725.524719),
		GunShop = Vector3.new(-1647.94275, 43.56744, 677.389771),
		JailOut = Vector3.new(-888.098206, 53.4290771, -2621.21045),
		JailIn = Vector3.new(-898.185974, 53.8590355, -3103.92041),
		GarageBoat = Vector3.new(-162.186905, 11.1047783, 262.945068),
		CarDealer = Vector3.new(208.97644, 26.1537762, -619.521973),
		GasStation = Vector3.new(456.351013, 26.0267735, 1004.63342),
		HeroBase = Vector3.new(-1749, 75, 1601),
		VolcanoTop = Vector3.new(-1674.02576, 366.245483, 1579.44983),
	}

	tptoplayer = Teleport_:AddBox("Tp to player", function()
		local Target = GetPlayerByAproxName(tptoplayer.Text);

		if Target then
			Teleport(Target.Character.PrimaryPart.Position);
			tptoplayer.Text = "";
		end;
	end)

	for i, v in pairs(Locations) do
		Teleport_:AddButton(i, function()
			Teleport(v);
		end);
	end;

	Teleport_:AddButton("PyramidIn", function()
		Teleport(workspace.Pyramid.Tele.Core1.Position)
		wait(0.1)
		Teleport(Vector3.new(993.201172, 13272.9795, 529.529175))
	end)
end

local CameraShaker = require(ReplicatedStorage.Modules.CameraShaker);
CameraShaker.ShakeOnce = function() end;
CameraShaker.Shake = function() end;

for i, v in pairs(workspace:GetDescendants()) do 
	if v:IsA("Sound") and v.Name == "Laser" then 
		v.Parent:Destroy()
	end;
end;
workspace.Lava:Destroy();

Notif("Loaded","[STR_ENCRYPT]MadCityHaxx V2 loaded created by <Color=Red>spidercraft781<Color=/> and <Color=Blue>Aztup<Color=/>.")
wait(0.3)
Notif("Close the gui","To close the gui press Right Shift.")

for i, v in pairs(frames) do
	v.Visible = true;
end;

UIS.InputBegan:Connect(function(Key, _)
	if Key.KeyCode == Enum.KeyCode.RightShift and not _ then
		library.gui.Enabled = not library.gui.Enabled;
	end;
end);

--
--
--
--
--
--
--
--
--
--
--
--
--
--