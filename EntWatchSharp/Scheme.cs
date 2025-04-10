namespace EntWatchSharp
{
	internal class Scheme
	{
		public string color_tag { get; set; }
		public string color_name { get; set; }
		public string color_steamid { get; set; }
		public string color_use { get; set; }
		public string color_pickup { get; set; }
		public string color_drop { get; set; }
		public string color_disconnect { get; set; }
		public string color_death { get; set; }
		public string color_warning { get; set; }
		public string color_enabled { get; set; }
		public string color_disabled { get; set; }

		public string server_name { get; set; }

		public Scheme()
		{
			color_tag =			"{green}";
			color_name =		"{default}";
			color_steamid =		"{grey}";
			color_use =			"{lightblue}";
			color_pickup =		"{lime}";
			color_drop =		"{pink}";
			color_disconnect =	"{orange}";
			color_death =		"{orange}";
			color_warning =		"{orange}";
			color_enabled =		"{green}";
			color_disabled =	"{red}";

			server_name =		"Zombies Server";
		}
	}
}
