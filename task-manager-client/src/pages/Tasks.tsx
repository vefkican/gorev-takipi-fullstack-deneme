import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { taskService, type TaskItem, type PagedResult } from "../services/api";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

function Tasks() {
  const [pagedResult, setPagedResult] = useState<PagedResult<TaskItem> | null>(
    null,
  );
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [search, setSearch] = useState("");
  const [filter, setFilter] = useState<boolean | undefined>(undefined);
  const [page, setPage] = useState(1);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    fetchTasks();
  }, [search, filter, page]);

  const fetchTasks = async () => {
    try {
      const response = await taskService.getAll(search, filter, page, 10);
      setPagedResult(response.data);
    } catch {
      setError("Tasklar yüklenemedi!");
    }
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await taskService.create({
        title,
        description,
        dueDate: dueDate || undefined,
      });
      setTitle("");
      setDescription("");
      setDueDate("");
      fetchTasks();
    } catch {
      setError("Task eklenemedi!");
    } finally {
      setLoading(false);
    }
  };

  const handleComplete = async (task: TaskItem) => {
    try {
      await taskService.update(task.id, {
        ...task,
        isCompleted: !task.isCompleted,
      });
      fetchTasks();
    } catch {
      setError("Güncelleme başarısız!");
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await taskService.delete(id);
      fetchTasks();
    } catch {
      setError("Silme başarısız!");
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    navigate("/login");
  };

  const formatDate = (date?: string) => {
    if (!date) return null;
    return new Date(date).toLocaleDateString("tr-TR");
  };

  const isOverdue = (date?: string) => {
    if (!date) return false;
    return new Date(date) < new Date();
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-2xl mx-auto py-10 px-4">
        {/* Header */}
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-3xl font-bold">Task Manager</h1>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => navigate("/profile")}>
              Profil
            </Button>
            <Button variant="outline" onClick={handleLogout}>
              Çıkış Yap
            </Button>
          </div>
        </div>

        {error && (
          <p className="text-sm text-red-500 bg-red-50 p-3 rounded-lg mb-4">
            {error}
          </p>
        )}

        {/* Task Ekleme Formu */}
        <Card className="mb-6">
          <CardHeader>
            <CardTitle className="text-lg">Yeni Task Ekle</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleCreate} className="space-y-3">
              <Input
                type="text"
                placeholder="Task başlığı"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
              />
              <Input
                type="text"
                placeholder="Açıklama (isteğe bağlı)"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
              <Input
                type="date"
                value={dueDate}
                onChange={(e) => setDueDate(e.target.value)}
              />
              <Button type="submit" className="w-full" disabled={loading}>
                {loading ? "Ekleniyor..." : "Task Ekle"}
              </Button>
            </form>
          </CardContent>
        </Card>

        {/* Arama ve Filtreleme */}
        <div className="flex gap-2 mb-4">
          <Input
            type="text"
            placeholder="Task ara..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
            className="flex-1"
          />
          <Button
            variant={filter === undefined ? "default" : "outline"}
            onClick={() => {
              setFilter(undefined);
              setPage(1);
            }}
          >
            Hepsi
          </Button>
          <Button
            variant={filter === false ? "default" : "outline"}
            onClick={() => {
              setFilter(false);
              setPage(1);
            }}
          >
            Aktif
          </Button>
          <Button
            variant={filter === true ? "default" : "outline"}
            onClick={() => {
              setFilter(true);
              setPage(1);
            }}
          >
            Tamamlanan
          </Button>
        </div>

        {/* Task Listesi */}
        <div className="space-y-3">
          {pagedResult?.items.length === 0 ? (
            <p className="text-center text-gray-400 py-10">
              Henüz task eklenmemiş!
            </p>
          ) : (
            pagedResult?.items.map((task) => (
              <Card
                key={task.id}
                className={task.isCompleted ? "opacity-60" : ""}
              >
                <CardContent className="flex items-center justify-between py-4">
                  <div>
                    <p
                      className={`font-medium ${task.isCompleted ? "line-through text-gray-400" : ""}`}
                    >
                      {task.title}
                    </p>
                    {task.description && (
                      <p className="text-sm text-gray-500 mt-1">
                        {task.description}
                      </p>
                    )}
                    {task.dueDate && (
                      <p
                        className={`text-xs mt-1 ${isOverdue(task.dueDate) && !task.isCompleted ? "text-red-500" : "text-gray-400"}`}
                      >
                        📅 {formatDate(task.dueDate)}
                        {isOverdue(task.dueDate) &&
                          !task.isCompleted &&
                          " — Süresi geçti!"}
                      </p>
                    )}
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleComplete(task)}
                    >
                      {task.isCompleted ? "↩ Geri Al" : "✓ Tamamla"}
                    </Button>
                    <Button
                      variant="destructive"
                      size="sm"
                      onClick={() => handleDelete(task.id)}
                    >
                      Sil
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))
          )}
        </div>

        {/* Pagination */}
        {pagedResult && pagedResult.totalPages > 1 && (
          <div className="flex items-center justify-between mt-6">
            <Button
              variant="outline"
              onClick={() => setPage((p) => p - 1)}
              disabled={!pagedResult.hasPreviousPage}
            >
              ← Önceki
            </Button>
            <span className="text-sm text-gray-500">
              {pagedResult.page} / {pagedResult.totalPages} sayfa (
              {pagedResult.totalCount} task)
            </span>
            <Button
              variant="outline"
              onClick={() => setPage((p) => p + 1)}
              disabled={!pagedResult.hasNextPage}
            >
              Sonraki →
            </Button>
          </div>
        )}
      </div>
    </div>
  );
}

export default Tasks;
