import { useTranslation } from "react-i18next";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { MoreHorizontal, Edit, Trash2, Shield, Users } from "lucide-react";

export default function RolesPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const { data: roles, isLoading } = useQuery({
    queryKey: ["roles"],
    queryFn: async () => {
      const response = await fetch("/api/admin/roles");
      if (!response.ok) throw new Error("Failed to fetch roles");
      return response.json();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async (roleId: string) => {
      const response = await fetch(`/api/admin/roles/${roleId}`, {
        method: "DELETE",
      });
      if (!response.ok) throw new Error("Failed to delete role");
      return response.json();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["roles"] });
    },
  });

  if (isLoading) {
    return <div className="flex items-center justify-center h-64">{t("common.loading")}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">{t("pages.roles")}</h1>
          <p className="text-gray-600">{t("descriptions.roles")}</p>
        </div>
        <Button>{t("roles.create_role")}</Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{t("roles.role_list")}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="mb-4">
            <Input
              placeholder={t("roles.search_placeholder")}
              className="max-w-sm"
            />
          </div>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t("roles.name")}</TableHead>
                <TableHead>{t("roles.description")}</TableHead>
                <TableHead>{t("roles.permissions")}</TableHead>
                <TableHead>{t("roles.users")}</TableHead>
                <TableHead>{t("roles.created_at")}</TableHead>
                <TableHead className="text-right">{t("common.actions")}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {roles?.map((role: any) => (
                <TableRow key={role.id}>
                  <TableCell>
                    <div className="flex items-center space-x-3">
                      <Shield className="h-4 w-4" />
                      <div>
                        <div className="font-medium">{role.name}</div>
                        <div className="text-sm text-gray-500">{role.code}</div>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>{role.description}</TableCell>
                  <TableCell>
                    <div className="flex flex-wrap gap-1">
                      {role.permissions?.slice(0, 3).map((permission: string, index: number) => (
                        <Badge key={index} variant="outline" className="text-xs">
                          {permission}
                        </Badge>
                      ))}
                      {role.permissions?.length > 3 && (
                        <Badge variant="outline" className="text-xs">
                          +{role.permissions.length - 3}
                        </Badge>
                      )}
                    </div>
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center space-x-2">
                      <Users className="h-4 w-4" />
                      <span>{role.userCount || 0}</span>
                    </div>
                  </TableCell>
                  <TableCell>
                    {new Date(role.createdAt).toLocaleDateString()}
                  </TableCell>
                  <TableCell className="text-right">
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="sm">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem>
                          <Edit className="mr-2 h-4 w-4" />
                          {t("common.edit")}
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          className="text-red-600"
                          onClick={() => deleteMutation.mutate(role.id)}
                        >
                          <Trash2 className="mr-2 h-4 w-4" />
                          {t("common.delete")}
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
