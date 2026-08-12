import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TaskService } from '../../core/services/task.service';

interface TodoTask {
  id: string;
  title: string;
  isCompleted: boolean;
  categoryId: string;
  categoryName?: string;
  deadline?: string | null;
  createdAt?: string;
  updatedAt?: string;
}

@Component({
  selector: 'app-tasks',
  templateUrl: './tasks.component.html',
  styleUrl: './tasks.component.css',
  imports: [ReactiveFormsModule]
})
export class TasksComponent implements OnInit {
  tasks: any[] = [];
  filteredTasks: any[] = [];
  categories: any[] = [];
  selectedCategoryId = '';
  selectedStatus: 'all' | 'completed' | 'uncompleted' = 'all';
  errorMessage: string | null = null;
  taskForm = new FormGroup({
    title: new FormControl('', Validators.required),
    categoryId: new FormControl('', Validators.required)
  });
  selectedTaskToEdit: TodoTask | null = null;
  isEditModalOpen = false;
  editTaskForm = new FormGroup({
    title: new FormControl('', Validators.required),
    categoryId: new FormControl('', Validators.required)
  });

  constructor(private taskService: TaskService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.getTasks();
    this.getCategories();
  }

  private getTasks(): void {
    this.taskService.getTasks().subscribe({
      next: (data) => {
        this.tasks = [...data.items];
        this.applyFilters();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = this.getErrorMessage(err);
        console.error('Failed to load tasks:', err);
        this.cdr.detectChanges();
      }
    });
  }

  private getCategories(): void {
    this.taskService.getCategories().subscribe({
      next: (data) => {
        this.categories = [...data];
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = this.getErrorMessage(err);
        console.error('Failed to load categories:', err);
        this.cdr.detectChanges();
      }
    });
  }

  onCategoryFilterChange(event: Event): void {
    this.selectedCategoryId = (event.target as HTMLSelectElement).value;
    this.applyFilters();
  }

  onStatusFilterChange(status: 'all' | 'completed' | 'uncompleted'): void {
    this.selectedStatus = status;
    this.applyFilters();
  }

  private applyFilters(): void {
    this.filteredTasks = this.tasks.filter((task) => {
      const matchesCategory = !this.selectedCategoryId || task.categoryId === this.selectedCategoryId;
      const matchesStatus =
        this.selectedStatus === 'all' ||
        (this.selectedStatus === 'completed' && task.isCompleted) ||
        (this.selectedStatus === 'uncompleted' && !task.isCompleted);
      return matchesCategory && matchesStatus;
    });
  }

  onSubmit(): void {
    if (this.taskForm.invalid) {
      this.taskForm.markAllAsTouched();
      return;
    }

    this.errorMessage = null;

    const { title, categoryId } = this.taskForm.value as { title: string; categoryId: string };

    this.taskService.addTask({ title, categoryId }).subscribe({
      next: () => {
        this.getTasks();
        this.taskForm.reset();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = this.getErrorMessage(err);
        console.error('Task creation failed:', err);
        this.cdr.detectChanges();
      }
    });
  }

  onDeleteTask(id: string): void {
    this.taskService.deleteTask(id).subscribe({
      next: () => {
        this.getTasks();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = this.getErrorMessage(err);
        console.error('Task deletion failed:', err);
        this.cdr.detectChanges();
      }
    });
  }

  onToggleComplete(task: any): void {
    task.isCompleted = !task.isCompleted;
    this.applyFilters();
    this.taskService.updateTask(task.id, task).subscribe({
      next: () => {
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = this.getErrorMessage(err);
        console.error('Task update failed:', err);
        this.cdr.detectChanges();
      }
    });
  }

  openEditModal(task: any): void {
    this.selectedTaskToEdit = { ...task };
    this.editTaskForm.setValue({
      title: task.title ?? '',
      categoryId: task.categoryId ?? ''
    });
    this.isEditModalOpen = true;
  }

  closeEditModal(): void {
    this.isEditModalOpen = false;
    this.selectedTaskToEdit = null;
  }

  saveEditModal(): void {
    if (!this.selectedTaskToEdit || this.editTaskForm.invalid) {
      this.editTaskForm.markAllAsTouched();
      return;
    }

    const { title, categoryId } = this.editTaskForm.value as { title: string; categoryId: string };
    this.errorMessage = null;

    this.taskService.updateTask(this.selectedTaskToEdit.id, { ...this.selectedTaskToEdit, title, categoryId }).subscribe({
      next: () => {
        this.getTasks();
        this.closeEditModal();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = this.getErrorMessage(err);
        console.error('Task update failed:', err);
        this.cdr.detectChanges();
      }
    });
  }

  getCategoryEmoji(categoryName: string | null | undefined): string {
    const emojiMap: Record<string, string> = {
      Work: '💼',
      Personal: '🏠',
      Study: '📚'
    };
    return emojiMap[categoryName ?? ''] ?? '📌';
  }

  private getErrorMessage(err: any): string {
    const backendMessage = err?.error?.message;
    if (typeof backendMessage === 'string' && backendMessage.trim()) {
      return backendMessage;
    }

    switch (err?.status) {
      case 400:
        return 'The request was invalid. Check the form fields and try again.';
      case 401:
        return 'Your session has expired. Please log in again.';
      case 404:
        return 'The requested resource was not found.';
      case 500:
        return 'The server encountered an error. Please try again later.';
      default:
        return 'Something went wrong. Please try again.';
    }
  }
}
