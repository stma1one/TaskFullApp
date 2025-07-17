﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskBaseApp.Models;
using TaskBaseApp.Service;

namespace TaskBaseApp.ViewModels;

public class UserTasksPageViewModel:ViewModelBase
{
	// Add properties, commands, and methods for the UserTasksPage functionality
	#region Fields
	ITaskServices _taskService;// Service for task management
	List<UserTask> userTask=new(); // Represents a User task
	Task loadData;// Represents a task for loading data

	ObservableCollection<ObservableUserTask> _allUserTasks=new(); // Collection of User tasks for binding to the UI
	ObservableCollection<ObservableUserTask> _filteredUserTasks = new(); // Collection of completed User tasks for binding to the UI
	bool _isLoading = false; // Indicates whether data is currently being loaded
	int userId;
        bool _hasError=false;
	string _searchText;
	#endregion


	#region Properties
	public ObservableCollection<ObservableUserTask> Tasks{get; set;} 
	public bool IsLoading{
		get => _isLoading;
		set
		{
			if (_isLoading != value)
			{
				_isLoading = value;
				OnPropertyChanged();
				(ClearFilterCommand as Command)?.ChangeCanExecute(); // Update the command state when SearchText changes


			}
		}
	}

	
	public int UserId
	{
		get => userId;
		set
		{
			if (userId != value)
			{
				userId = value;
				OnPropertyChanged();
			}
		}
	}

	public string SearchText
	{
		get => _searchText;
		set
		{
			if (_searchText != value)
			{
				_searchText = value;
				OnPropertyChanged();
				(ClearFilterCommand as Command)?.ChangeCanExecute(); // Update the command state when SearchText changes

			}
		}
	}

 	public bool HasError
  {
  get=>_hasError;
  set
  {
  	_hasError=value;
        OnPropertyChanged();
  }
  }

	#endregion
	#region Commands
	public ICommand LoadTasksCommand
	{
		get;
	}
	public ICommand FilterTaskCommand
	{
		get;
	}
	public ICommand ClearFilterCommand
	{
		get;
	}
	public ICommand ChangeTaskDescriptionCommand
	{
		get;
	}
	public ICommand DeleteTaskCommand
	{
		get;
	}
	#endregion
	#region Constructor
	public UserTasksPageViewModel(ITaskServices services)
	{
		// Initialize properties or commands here if needed
		UserId = 1;
		_taskService = services;
		Tasks = new();
		LoadTasksCommand = new Command(async () => await LoadUserTasksAsync());
		FilterTaskCommand = new Command<string>(async (query) => await FilterTasks(query));
		ClearFilterCommand = new Command(async () => await FilterTasks(string.Empty),()=>string.IsNullOrEmpty(SearchText)&&!IsLoading);
		ChangeTaskDescriptionCommand = new Command(() => { if (Tasks.Count > 0) { Tasks[0].TaskDescription = "וואחד שינוי"; } });
		DeleteTaskCommand = new Command<ObservableUserTask>(DeleteTask);
		loadData =LoadUserTasksAsync();
	}
	#endregion

	#region Methods
	private async Task FilterTasks(string query)
	{
		IsLoading = true;
		

		if (!string.IsNullOrEmpty(query))
		{
			_filteredUserTasks = new ObservableCollection<ObservableUserTask>(_allUserTasks.Where(x => x.TaskDescription.Contains(query)));
			Application.Current?.Dispatcher.Dispatch(() =>
			{
				Tasks.Clear();
				foreach (var task in _filteredUserTasks)
				{
					Tasks.Add(task);
				}
			});
			IsLoading = false;
			return;
		}
		await LoadUserTasksAsync();
		IsLoading = false;
		


	}
	/// <summary>
	/// Loads User tasks from the service.
	/// </summary>
	public async Task LoadUserTasksAsync()
	{
		IsLoading = true;
		try
		{
			userTask.Clear();
			_allUserTasks.Clear();
			userTask = await _taskService.GetTasks(UserId); // Assuming 1 is the User ID
			if (userTask != null && userTask.Count > 0)
			{
				foreach (var task in userTask)
				{
					_allUserTasks.Add(new ObservableUserTask(task));
				}
			}
			
				Tasks.Clear();
				foreach (var task in _allUserTasks)
				{
					Tasks.Add(task);
				}
			await Task.Delay(500);
			IsLoading = false;
			HasError = false;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error loading tasks: {ex.Message}");
   			HasError=true;
		}
		finally
		{
			IsLoading = false;
			IsBusy = false;
		}
	}
	private void DeleteTask(ObservableUserTask task)
	{
		if (task == null) return; // Ensure task is not null
		Tasks.Remove(task);
	}
	#endregion
}
