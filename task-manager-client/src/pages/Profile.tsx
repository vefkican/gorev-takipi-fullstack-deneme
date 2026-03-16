import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { taskService } from "../services/api";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card";

interface ProfileStats {
  total: number;
  completed: number;
  active: number;
  overdue: number;
}

function Profile() {
  const [stats, setStats] = useState<ProfileStats>({
    total: 0,
    completed: 0,
    active: 0,
    overdue: 0,
  });
  const [username, setUsername] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    // Token'dan username al
    const token = localStorage.getItem("token");
    if (token) {
      const payload = JSON.parse(atob(token.split(".")[1]));
      setUsername(
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
      );
    }

    // eslint-disable-next-line react-hooks/immutability
    fetchStats();
  }, []);

  const fetchStats = async () => {
    try {
      const response = await taskService.getAll(undefined, undefined, 1, 1000);
      const tasks = response.data.items; // ← .items ekledik
      const now = new Date();

      setStats({
        total: response.data.totalCount, // ← totalCount kullandık
        completed: tasks.filter((t) => t.isCompleted).length,
        active: tasks.filter((t) => !t.isCompleted).length,
        overdue: tasks.filter(
          (t) => !t.isCompleted && t.dueDate && new Date(t.dueDate) < now,
        ).length,
      });
    } catch {
      console.error("Stats yüklenemedi");
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/login");
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-2xl mx-auto py-10 px-4">
        {/* Header */}
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-3xl font-bold">Profil</h1>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => navigate("/tasks")}>
              ← Tasklar
            </Button>
            <Button variant="outline" onClick={handleLogout}>
              Çıkış Yap
            </Button>
          </div>
        </div>

        {/* Kullanıcı Bilgisi */}
        <Card className="mb-6">
          <CardHeader>
            <div className="flex items-center gap-4">
              <div className="w-16 h-16 rounded-full bg-primary flex items-center justify-center text-white text-2xl font-bold">
                {username.charAt(0).toUpperCase()}
              </div>
              <div>
                <CardTitle className="text-xl">{username}</CardTitle>
                <CardDescription>Task Manager Kullanıcısı</CardDescription>
              </div>
            </div>
          </CardHeader>
        </Card>

        {/* İstatistikler */}
        <div className="grid grid-cols-2 gap-4">
          <Card>
            <CardContent className="pt-6">
              <p className="text-4xl font-bold text-primary">{stats.total}</p>
              <p className="text-sm text-gray-500 mt-1">Toplam Task</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="pt-6">
              <p className="text-4xl font-bold text-green-500">
                {stats.completed}
              </p>
              <p className="text-sm text-gray-500 mt-1">Tamamlanan</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="pt-6">
              <p className="text-4xl font-bold text-blue-500">{stats.active}</p>
              <p className="text-sm text-gray-500 mt-1">Aktif Task</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="pt-6">
              <p className="text-4xl font-bold text-red-500">{stats.overdue}</p>
              <p className="text-sm text-gray-500 mt-1">Süresi Geçmiş</p>
            </CardContent>
          </Card>
        </div>

        {/* Tamamlanma Oranı */}
        {stats.total > 0 && (
          <Card className="mt-4">
            <CardHeader>
              <CardTitle className="text-lg">Tamamlanma Oranı</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="w-full bg-gray-200 rounded-full h-4">
                <div
                  className="bg-green-500 h-4 rounded-full transition-all"
                  style={{
                    width: `${Math.round((stats.completed / stats.total) * 100)}%`,
                  }}
                />
              </div>
              <p className="text-sm text-gray-500 mt-2">
                {Math.round((stats.completed / stats.total) * 100)}% tamamlandı
              </p>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}

export default Profile;
