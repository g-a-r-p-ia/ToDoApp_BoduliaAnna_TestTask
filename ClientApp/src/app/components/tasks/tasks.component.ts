import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
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
  imports: [ReactiveFormsModule, FormsModule]
})
export class TasksComponent implements OnInit {
  tasks: any[] = [];
  categories: any[] = [];
  selectedCategoryId = '';
  selectedStatus: 'all' | 'completed' | 'uncompleted' = 'all';
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  searchTerm = '';
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
    this.loadTasks();
    this.getCategories();
  }

  private loadTasks(): void {
    const isCompleted =
      this.selectedStatus === 'completed' ? true : this.selectedStatus === 'uncompleted' ? false : null;

    this.taskService
      .getTasks(this.currentPage, this.pageSize, this.selectedCategoryId || undefined, this.searchTerm, isCompleted)
      .subscribe({
        next: (data) => {
          this.tasks = [...data.items];
          this.totalCount = data.totalCount;
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
    this.currentPage = 1;
    this.loadTasks();
  }

  onStatusFilterChange(status: 'all' | 'completed' | 'uncompleted'): void {
    this.selectedStatus = status;
    this.currentPage = 1;
    this.loadTasks();
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadTasks();
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadTasks();
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadTasks();
    }
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
        this.currentPage = 1;
        this.loadTasks();
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
        if (this.tasks.length === 1 && this.currentPage > 1) {
          this.currentPage--;
        }
        this.loadTasks();
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
    this.taskService.updateTask(task.id, task).subscribe({
      next: () => {
        this.loadTasks();
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
        this.currentPage = 1;
        this.loadTasks();
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
