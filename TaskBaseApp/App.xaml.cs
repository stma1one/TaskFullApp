﻿using TaskBaseApp.Views;

namespace TaskBaseApp
{
    public partial class App : Application
    {
		Page? firstpage;
		public App(IServiceProvider provider)
		{
			InitializeComponent();
			// Initialize the first page of the application
			this.firstpage =provider.GetService<UserTasksPage>();
		}

		protected override Window CreateWindow(IActivationState? activationState)
		{
			// return new Window(new AppShell());
			return new Window(firstpage);
		}
	}
}