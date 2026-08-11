import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TaskService } from '../../core/services/task.service';

@Component({
  selector: 'app-tasks',
  templateUrl: './tasks.component.html',
  styleUrl: './tasks.component.css',
  imports: [ReactiveFormsModule]
})
export class TasksComponent implements OnInit {
  tasks: any[] = [];
  taskForm = new FormGroup({
    title: new FormControl('', Validators.required)
  });

  constructor(private taskService: TaskService) {}

  ngOnInit(): void {
    this.taskService.getTasks().subscribe((data) => {
      this.tasks = data;
    });
  }

  onSubmit(): void {
    if (this.taskForm.valid) {
      this.taskService.addTask(this.taskForm.value.title!).subscribe((task) => {
        this.tasks.push(task);
        this.taskForm.reset();
      });
    }
  }
}
