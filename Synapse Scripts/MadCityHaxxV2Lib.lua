-- Wally library modified by Aztup
-- Design idea by 7th
-- You don't have the perm to use it without the perm of Aztup

local Heartbeat = game:GetService("RunService").Heartbeat;
local library = {
	windowcount = 0;
}

local defaults = defaults or {
	boxcolor = nil,
	Rainbow = true,
	slidingbarcolor = nil,
	txtcolor = Color3.fromRGB(255, 255, 255),
	underline = Color3.fromRGB(0, 255, 140),
	barcolor = Color3.fromRGB(50, 50, 50),
	bgcolor = Color3.fromRGB(50, 50, 50)
};

if game:GetService("Players").LocalPlayer.PlayerGui:FindFirstChild("Aztup's UI") then
	game:GetService("Players").LocalPlayer.PlayerGui:FindFirstChild("Aztup's UI"):Destroy();
end

local tweenPosition = function (object, newpos)
	object.Position = newpos
end;

local dragger = {};
local resizer = {};
local frames = {};
local Callbacks = {};
local Enabled = {};

do
	local mouse = game:GetService("Players").LocalPlayer:GetMouse();
	local inputService = game:GetService('UserInputService');
	
	function dragger.new(frame)
		local s, event = pcall(function()
			return frame.MouseEnter;
		end);

		if s then
			frame.Active = true;

			event:connect(function()
				local input = frame.InputBegan:connect(function(key)
					if key.UserInputType == Enum.UserInputType.MouseButton1 then
						local objectPosition = Vector2.new(mouse.X - frame.AbsolutePosition.X, mouse.Y - frame.AbsolutePosition.Y);
						while Heartbeat:wait() and inputService:IsMouseButtonPressed(Enum.UserInputType.MouseButton1) do
							frame:TweenPosition(UDim2.new(0, mouse.X - objectPosition.X + (frame.Size.X.Offset * frame.AnchorPoint.X), 0, mouse.Y - objectPosition.Y + (frame.Size.Y.Offset * frame.AnchorPoint.Y)), 'Out', 'Quad', 0, true);
						end;
					end;
				end);

				local leave;
				leave = frame.MouseLeave:connect(function()
					input:disconnect();
					leave:disconnect();
				end)
			end)
		end
	end

	function resizer.new(p, s)
		p:GetPropertyChangedSignal('AbsoluteSize'):connect(function()
			s.Size = UDim2.new(s.Size.X.Scale, s.Size.X.Offset, s.Size.Y.Scale, p.AbsoluteSize.Y);
		end)
	end
end;

function library:Create(class, props)
	local object = Instance.new(class);
	
	for i, prop in next, props do
		if i ~= "Parent" then
			object[i] = prop;
		end
	end

	object.Parent = props.Parent;
	return object;
end


function library:CreateWindow(options)
	assert(options.text, "no name");
	local window = {
		count = 0;
		toggles = {},
		closed = false;
	}

	local options = options or {};
	setmetatable(options, {__index = defaults})

	self.windowcount = self.windowcount + 1;

    library.gui = library.gui or self:Create("ScreenGui", {Name = "Aztup's UI", Parent = game:GetService("Players").LocalPlayer.PlayerGui})
    library.gui.ResetOnSpawn = false

	window.frame = self:Create("Frame", {
		Name = options.text;
		Parent = self.gui,
		Visible = false,
		Active = true,
		BackgroundTransparency = 0,
		Size = UDim2.new(0, 178, 0, 30),
		Position = UDim2.new(0, (5 + ((178 * self.windowcount) - 178)), 0, 5),
		BackgroundColor3 = options.barcolor,
		BorderSizePixel = 0
	});

	frames[#frames + 1] = window.frame;

	window.background = self:Create('Frame', {
		Name = 'Background';
		Parent = window.frame,
		BorderSizePixel = 0;
		BackgroundColor3 = options.bgcolor,
		Position = UDim2.new(0, 0, 1, 0),
		Size = UDim2.new(1, 0, 0, 25),
		BackgroundTransparency = 0.4,
		ClipsDescendants = true;
	})

	window.container = self:Create('ScrollingFrame', {
		Name = 'Container';
		Parent = window.frame,
		BorderSizePixel = 0,
		BorderSizePixel = 0,
		BackgroundColor3 = options.bgcolor,
		CanvasSize = UDim2.new(0, 0, 0, 0),
		BottomImage = "rbxasset://textures/ui/Scroll/scroll-middle.png",
		TopImage = "rbxasset://textures/ui/Scroll/scroll-middle.png",
		Position = UDim2.new(0, 0, 1, 0),
		Size = UDim2.new(1, 0, 0, 25),
		BackgroundTransparency = 0.4,
		ClipsDescendants = true,
		ScrollBarThickness = 10
	})

	window.organizer = self:Create('UIListLayout', {
		Name = 'Sorter';
		SortOrder = Enum.SortOrder.LayoutOrder;
		Parent = window.container;
	})

	window.padder = self:Create('UIPadding', {
		Name = 'Padding';
		PaddingLeft = UDim.new(0, 10);
		PaddingTop = UDim.new(0, 5);
		Parent = window.container;
	})

	local underline = self:Create("Frame", {
		Name = 'Underline';
		Size = UDim2.new(1, 0, 0, 3),
		Position = UDim2.new(0, 0, 1, -1),
		BorderSizePixel = 0;
		BackgroundColor3 = options.underline;
		Parent = window.frame
	})

	local togglebutton = self:Create("TextButton", {
		Name = 'Toggle';
		ZIndex = 2,
		BackgroundTransparency = 1;
		Position = UDim2.new(1, -25, 0, 0),
		Size = UDim2.new(0, 25, 1, 0),
		Text = "-",
		TextSize = 17,
		TextColor3 = options.txtcolor,
		Font = Enum.Font.SourceSans,
		Parent = window.frame,
	});

	togglebutton.MouseButton1Click:connect(function()
		window.closed = not window.closed;
		togglebutton.Text = (window.closed and "+" or "-");
		if window.closed then
			window:Resize(true, UDim2.new(1, 0, 0, 0));
			wait(0.4)
			window.container.Visible = false
		else
			window.container.Visible = true
			window:Resize(true);
		end;
	end);

	self:Create("TextLabel", {
		Size = UDim2.new(1, 0, 1, 0),
		BackgroundTransparency = 1,
		BorderSizePixel = 0,
		TextColor3 = options.txtcolor,
		TextColor3 = (options.bartextcolor or Color3.fromRGB(255, 255, 255)),
		TextSize = 17,
		Font = Enum.Font.SourceSansSemibold,
		Text = options.text or "window",
		Name = "Window",
		Parent = window.frame,
	})

	local open = {}

	do
		dragger.new(window.frame)
		resizer.new(window.background, window.container);
	end

	local function getSize()
		local ySize = 0;
		for i, object in next, window.container:GetChildren() do
			if (not object:IsA('UIListLayout')) and (not object:IsA('UIPadding')) then
				ySize = ySize + object.AbsoluteSize.Y
			end
		end
		
		if ySize > 400 then
			ySize = 400;
			window.container.CanvasSize = UDim2.new(0, 0, 0, (#window.container:GetChildren() - 1) * 20)
		end;
		
		return UDim2.new(1, 0, 0, ySize + 10)
	end

	function window:Resize(tween, change)
		local size = change or getSize()
		self.container.ClipsDescendants = true;

		if tween then
			self.background:TweenSize(size, "Out", "Sine", 0.5, true)
		else
			self.background.Size = size
		end
	end

	function window:AddSlidingBar(text, minval, maxval, callback)
		self.count = self.count + 1;
		callback = callback or function() end;
		minval = minval or 0;
		maxval = maxval or 100;

		maxval = maxval / 8;
		
		local newContainer = library:Create("Frame", {
			Parent = self.container,
			Size = UDim2.new(1, -10, 0, 20),
			BackgroundColor3 = options.slidingbarcolor,
			BackgroundTransparency = 1,
		})

		local slidingbar = library:Create("ScrollingFrame", {
			Parent = newContainer,
			Size = UDim2.new(1, -10, 0, 20),
			CanvasSize = UDim2.new(8, 0, 0, 0),
			BackgroundColor3 = options.slidingbarcolor,
			BottomImage = "rbxasset://textures/ui/Scroll/scroll-middle.png",
			TopImage = "rbxasset://textures/ui/Scroll/scroll-middle.png",
			MidImage = "rbxasset://textures/ui/Scroll/scroll-middle.png",
			ScrollBarThickness = 20,
			ScrollBarImageColor3 = Color3.fromRGB(0, 0, 0),
			ScrollBarImageTransparency = 0.3,
		});

		local label = library:Create("TextLabel", {
			Size = UDim2.new(1, 0, 1, 0),
			BackgroundTransparency = 1,
			BackgroundColor3 = options.slidingbarcolor,
			TextColor3 = Color3.fromRGB(255, 255, 255),
			TextXAlignment = Enum.TextXAlignment.Center,
			TextSize = 16,
			Text = text .. ": " .. minval,
			Font = Enum.Font.SourceSans,
			LayoutOrder = self.Count,
			BorderSizePixel = 0,
			Parent = newContainer,
		})

		slidingbar.Changed:Connect(function(p)
			if p ~= "CanvasPosition" then return end

			local Val = minval - math.floor((slidingbar.CanvasPosition.X / (slidingbar.CanvasSize.X.Offset - slidingbar.AbsoluteWindowSize.X)) * maxval) 
			label.Text = text .. ": " .. tostring(Val);
			callback(Val);
		end)

		self:Resize();

	end;

	function window:AddToggle(text, callback)
		self.count = self.count + 1;

		callback = callback or function() end;

		local label = library:Create("TextLabel", {
			Text =  text,
			Size = UDim2.new(1, -10, 0, 20),
			BackgroundTransparency = 1,
			TextColor3 = Color3.fromRGB(255, 255, 255),
			TextXAlignment = Enum.TextXAlignment.Left,
			LayoutOrder = self.Count,
			TextSize = 16,
			Font = Enum.Font.SourceSans,
			Parent = self.container,
		})

		local button = library:Create("TextButton", {
			Text = "OFF",
			TextColor3 = Color3.fromRGB(255, 25, 25),
			BackgroundTransparency = 1,
			Position = UDim2.new(1, -25, 0, 0),
			Size = UDim2.new(0, 25, 1, 0),
			TextSize = 17,
			Font = Enum.Font.SourceSansSemibold,
			Parent = label,
		})

		Callbacks[text] = function(toggle)
			if not toggle then
				toggle = not self.toggles[text];
			end;

			self.toggles[text] = toggle;
			Enabled[text] = toggle;
			button.TextColor3 = (self.toggles[text] and Color3.fromRGB(0, 255, 140) or Color3.fromRGB(255, 25, 25));
			button.Text = (self.toggles[text] and "ON" or "OFF");

			callback(self.toggles[text]);
        end;

        button.MouseButton1Click:connect(Callbacks[text]);

		self:Resize();
		return button;
	end

	function window:AddBox(text, callback)
		self.count = self.count + 1
		callback = callback or function() end

		local box = library:Create("TextBox", {
			PlaceholderText = text,
			Size = UDim2.new(1, -10, 0, 20),
			BackgroundTransparency = 0.75,
			BackgroundColor3 = options.boxcolor,
			TextColor3 = Color3.fromRGB(255, 255, 255),
			TextXAlignment = Enum.TextXAlignment.Center,
			TextSize = 16,
			Text = "",
			Font = Enum.Font.SourceSans,
			LayoutOrder = self.Count,
			BorderSizePixel = 0,
			Parent = self.container,
		})

		box.FocusLost:connect(function(...)
			callback(box, ...)
		end)

		self:Resize()
		return box
	end

	function window:AddButton(text, callback)
		self.count = self.count + 1

		callback = callback or function() end
		
		local button = library:Create("TextButton", {
			Text = text,
			Size = UDim2.new(1, -10, 0, 20),
			BackgroundTransparency = 1,
			TextColor3 = Color3.fromRGB(255, 255, 255),
			TextXAlignment = Enum.TextXAlignment.Left,
			TextSize = 16,
			Font = Enum.Font.SourceSans,
			LayoutOrder = self.Count,
			Parent = self.container,
		})

		button.MouseButton1Click:connect(callback)
		self:Resize()
		return button
	end

	function window:AddLabel(text)
		self.count = self.count + 1;

		local tSize = game:GetService('TextService'):GetTextSize(text, 16, Enum.Font.SourceSans, Vector2.new(math.huge, math.huge))

		local button = library:Create("TextLabel", {
			Text =  text,
			Size = UDim2.new(1, -10, 0, tSize.Y + 5),
			TextScaled = false,
			BackgroundTransparency = 1,
			TextColor3 = Color3.fromRGB(255, 255, 255),
			TextXAlignment = Enum.TextXAlignment.Left,
			TextSize = 16,
			Font = Enum.Font.SourceSans,
			LayoutOrder = self.Count,
			Parent = self.container,
		});

		self:Resize();
		return button;
	end;

	function window:AddDropdown(options, callback)
		self.count = self.count + 1
		local default = options[1] or "";

		callback = callback or function() end
		local dropdown = library:Create("TextLabel", {
			Size = UDim2.new(1, -10, 0, 20),
			BackgroundTransparency = 0.75,
			BackgroundColor3 = options.boxcolor,
			TextColor3 = Color3.fromRGB(255, 255, 255),
			TextXAlignment = Enum.TextXAlignment.Center,
			TextSize = 16,
			Text = default,
			Font = Enum.Font.SourceSans,
			BorderSizePixel = 0;
			LayoutOrder = self.Count,
			Parent = self.container,
		})

		local button = library:Create("ImageButton",{
			BackgroundTransparency = 1,
			Image = 'rbxassetid://3234893186',
			Size = UDim2.new(0, 18, 1, 0),
			Position = UDim2.new(1, -20, 0, 0),
			Parent = dropdown,
		})

		local frame;

		local function isInGui(frame)
			local mloc = game:GetService('UserInputService'):GetMouseLocation();
			local mouse = Vector2.new(mloc.X, mloc.Y - 36);

			local x1, x2 = frame.AbsolutePosition.X, frame.AbsolutePosition.X + frame.AbsoluteSize.X;
			local y1, y2 = frame.AbsolutePosition.Y, frame.AbsolutePosition.Y + frame.AbsoluteSize.Y;

			return (mouse.X >= x1 and mouse.X <= x2) and (mouse.Y >= y1 and mouse.Y <= y2);
		end;

		local function count(t)
			local c = 0;
			for i, v in next, t do
				c = c + 1;
			end;
			return c;
		end;

		button.MouseButton1Click:connect(function()
			if count(options) == 0 then
				return;
			end;

			if frame then
				frame:Destroy();
				frame = nil;
			end

			self.container.ClipsDescendants = false;

			frame = library:Create('Frame', {
				Position = UDim2.new(0, 0, 1, 0);
				BackgroundColor3 = Color3.fromRGB(40, 40, 40);
				Size = UDim2.new(0, dropdown.AbsoluteSize.X, 0, (count(options) * 21));
				BorderSizePixel = 0;
				Parent = dropdown;
				ClipsDescendants = true;
				ZIndex = 2;
			})

			library:Create('UIListLayout', {
				Name = 'Layout';
				Parent = frame;
			})

			for i, option in next, options do
				local selection = library:Create('TextButton', {
					Text = option;
					BackgroundColor3 = Color3.fromRGB(40, 40, 40);
					TextColor3 = Color3.fromRGB(255, 255, 255);
					BorderSizePixel = 0;
					TextSize = 16;
					Font = Enum.Font.SourceSans;
					Size = UDim2.new(1, 0, 0, 21);
					Parent = frame;
					ZIndex = 2;
				})

				selection.MouseButton1Click:connect(function()
					dropdown.Text = option;
					callback(option);
					frame.Size = UDim2.new(1, 0, 0, 0);
					game:GetService('Debris'):AddItem(frame, 0.1);
				end);
			end;
		end);

		game:GetService('UserInputService').InputBegan:connect(function(m)
			if m.UserInputType == Enum.UserInputType.MouseButton1 then
				if frame and (not isInGui(frame)) then
					game:GetService('Debris'):AddItem(frame);
				end
			end
		end)

		callback(default);
		self:Resize();
		return {
			Refresh = function(self, array)
				game:GetService('Debris'):AddItem(frame);
				options = array;
				dropdown.Text = options[1];
			end;
		};
	end;

	return window;
end;

if defaults.Rainbow then
    Heartbeat:Connect(function()
        for i, v in pairs(frames) do
            if v.Parent ~= nil then
                local r = (math.sin(workspace.DistributedGameTime/2)/2)+0.5
                local g = (math.sin(workspace.DistributedGameTime)/2)+0.5
                local b = (math.sin(workspace.DistributedGameTime*1.5)/2)+0.5
                local color = Color3.new(r, g, b);
                v.Underline.BackgroundColor3 = color;
            end;
        end;
    end);
end;

return library, frames, Callbacks, Enabled