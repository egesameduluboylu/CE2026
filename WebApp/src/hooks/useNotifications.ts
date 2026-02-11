import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useCallback, useEffect, useState } from "react";

export interface Notification {
  id: string;
  type: "info" | "success" | "warning" | "error";
  title: string;
  message: string;
  actionUrl?: string;
  actionText?: string;
  isRead: boolean;
  createdAt: string;
  readAt?: string;
  expiresAt?: string;
  metadata?: string;
}

export interface NotificationListResponse {
  notifications: Notification[];
  total: number;
  unreadCount: number;
}

const API_BASE = import.meta.env.VITE_API_URL || "http://localhost:5000";

export function useNotifications(page = 1, pageSize = 20, unreadOnly = false) {
  return useQuery<NotificationListResponse>({
    queryKey: ["notifications", page, pageSize, unreadOnly],
    queryFn: async () => {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
        unreadOnly: unreadOnly.toString(),
      });

      const response = await fetch(`${API_BASE}/api/notifications?${params}`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });

      if (!response.ok) {
        throw new Error("Failed to fetch notifications");
      }

      const data = await response.json();
      return data.data;
    },
  });
}

export function useUnreadCount() {
  return useQuery<number>({
    queryKey: ["notifications", "unread-count"],
    queryFn: async () => {
      const response = await fetch(`${API_BASE}/api/notifications/unread-count`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });

      if (!response.ok) {
        throw new Error("Failed to fetch unread count");
      }

      const data = await response.json();
      return data.data.count;
    },
    refetchInterval: 30000, // Refresh every 30 seconds
  });
}

export function useMarkAsRead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (notificationId: string) => {
      const response = await fetch(`${API_BASE}/api/notifications/${notificationId}/mark-read`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });

      if (!response.ok) {
        throw new Error("Failed to mark notification as read");
      }

      return response.json();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
      queryClient.invalidateQueries({ queryKey: ["notifications", "unread-count"] });
    },
  });
}

export function useMarkAllAsRead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      const response = await fetch(`${API_BASE}/api/notifications/mark-all-read`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });

      if (!response.ok) {
        throw new Error("Failed to mark all notifications as read");
      }

      return response.json();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
      queryClient.invalidateQueries({ queryKey: ["notifications", "unread-count"] });
    },
  });
}

export function useDeleteNotification() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (notificationId: string) => {
      const response = await fetch(`${API_BASE}/api/notifications/${notificationId}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });

      if (!response.ok) {
        throw new Error("Failed to delete notification");
      }

      return response.json();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
      queryClient.invalidateQueries({ queryKey: ["notifications", "unread-count"] });
    },
  });
}

export function useSignalRNotifications() {
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const queryClient = useQueryClient();

  useEffect(() => {
    const token = localStorage.getItem("accessToken");
    if (!token) return;

    const newConnection = new HubConnectionBuilder()
      .withUrl(`${API_BASE}/hub/notifications`, {
        accessTokenFactory: () => token!,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    newConnection.on("NotificationReceived", (notification: Notification) => {
      setNotifications(prev => [notification, ...prev]);
      setUnreadCount(prev => prev + 1);
      queryClient.invalidateQueries({ queryKey: ["notifications", "unread-count"] });
    });

    newConnection.on("NotificationMarkedAsRead", (notificationId: string) => {
      setNotifications(prev => 
        prev.map(n => n.id === notificationId ? { ...n, isRead: true } : n)
      );
      setUnreadCount(prev => Math.max(0, prev - 1));
      queryClient.invalidateQueries({ queryKey: ["notifications", "unread-count"] });
    });

    newConnection.on("AllNotificationsMarkedAsRead", () => {
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
      setUnreadCount(0);
      queryClient.invalidateQueries({ queryKey: ["notifications", "unread-count"] });
    });

    newConnection.on("NotificationDeleted", (notificationId: string) => {
      setNotifications(prev => prev.filter(n => n.id !== notificationId));
      queryClient.invalidateQueries({ queryKey: ["notifications", "unread-count"] });
    });

    newConnection.on("UnreadCountUpdated", (count: number) => {
      setUnreadCount(count);
      queryClient.invalidateQueries({ queryKey: ["notifications", "unread-count"] });
    });

    setConnection(newConnection);

    return () => {
      newConnection.stop();
    };
  }, [queryClient]);

  const startConnection = useCallback(async () => {
    if (connection && connection.state === "Disconnected") {
      try {
        await connection.start();
        console.log("SignalR Connected");
      } catch (err) {
        console.error("SignalR Connection Error: ", err);
      }
    }
  }, [connection]);

  const stopConnection = useCallback(async () => {
    if (connection && connection.state === "Connected") {
      await connection.stop();
      console.log("SignalR Disconnected");
    }
  }, [connection]);

  return {
    connection,
    notifications,
    unreadCount,
    startConnection,
    stopConnection,
  };
}
