package com.qianxi.afnative.Function;

import android.app.Activity;
import android.content.pm.PackageManager;
import android.util.Log;

import com.qianxi.afnative.Tool.Permission.OnPermission;
import com.qianxi.afnative.Tool.Permission.Permission;
import com.qianxi.afnative.Tool.Permission.XXPermissions;

import java.util.Arrays;
import java.util.List;

/*
动态请求权限
 */
public class DynamicPermission {
    public void GetRecordVedioPermission(final Activity activity,PermissionCallBack permissionCallBack)
    {
        String[] permission = new String[]{
                Permission.ACCESS_FINE_LOCATION,
                Permission.READ_EXTERNAL_STORAGE,
                Permission.WRITE_EXTERNAL_STORAGE,
                Permission.RECORD_AUDIO
        };

        if(isHasPermission(activity, permission))
        {
            permissionCallBack.GetPermissionFinish("2");
        }
        else
        {
            RequestPermission(activity,permission,permissionCallBack);
        }
    }
    public void GetCapturePermission(final Activity activity,PermissionCallBack permissionCallBack)
    {
        String[] permission = new String[]{
            Permission.ACCESS_FINE_LOCATION,
            Permission.READ_EXTERNAL_STORAGE,
            Permission.WRITE_EXTERNAL_STORAGE
        };
        if(isHasPermission(activity,permission))
        {
            permissionCallBack.GetPermissionFinish("2");
        }
        else
        {
            RequestPermission(activity, permission,permissionCallBack);
        }
    }
    //获取相册所需权限
    public void GetAlbumPermission(final Activity activity,PermissionCallBack permissionCallBack)
    {
        String[] permission = new String[]{
                Permission.ACCESS_FINE_LOCATION
        };
        if(isHasPermission(activity, permission))
        {
            permissionCallBack.GetPermissionFinish("2");
        }
        else
        {
            RequestPermission(activity, permission,permissionCallBack);
        }
    }
    //获取存储读写权限
    public void GetStoragePermission(final Activity activity, PermissionCallBack permissionCallBack)
    {
        if(isHasPermission(activity, Permission.Group.STORAGE))
        {
            permissionCallBack.GetPermissionFinish("2");
        }
        else
        {
            RequestPermission(activity, Permission.Group.STORAGE,permissionCallBack);
        }
    }
    //获取录音权限
    public void GetAudioPermission(final Activity activity, PermissionCallBack permissionCallBack)
    {
//输出Androidmanifest中的权限,是必要的条件
//        List<String> manifestPermissions = null;
//        try {
//            manifestPermissions = Arrays.asList(activity.getApplicationContext().getPackageManager()
//                    .getPackageInfo(activity.getApplicationContext().getPackageName(),
//                            PackageManager.GET_PERMISSIONS).requestedPermissions);
//        } catch (PackageManager.NameNotFoundException e) {
//            e.printStackTrace();
//        }
//        if (manifestPermissions != null && !manifestPermissions.isEmpty()) {
//            for (String permissi : manifestPermissions) {
//                Log.d("GetStoragePermission", permissi);
//            }
//        }

        if(isHasPermission(activity, new String[]{Permission.RECORD_AUDIO}))
        {
            permissionCallBack.GetPermissionFinish("2");
        }
        else
        {
            RequestPermission(activity, new String[]{Permission.RECORD_AUDIO},permissionCallBack);
        }
    }

    public boolean isHasPermission(final Activity currentActivity, String[] pemission) {
        if (XXPermissions.isHasPermission(currentActivity, pemission)) {
            return true;
        }else {
            return false;
        }
    }
    //跳转到权限设置页面
    public void gotoPermissionSettings(final Activity currentActivity) {
        XXPermissions.gotoPermissionSettings(currentActivity);
    }
    public void RequestPermission(final Activity currentActivity,final String[] pemission,final PermissionCallBack permissionCallBack)
    {
        XXPermissions.with(currentActivity)
                // 可设置被拒绝后继续申请，直到用户授权或者永久拒绝
                //.constantRequest()
                // 支持请求6.0悬浮窗权限8.0请求安装权限
                //.permission(Permission.REQUEST_INSTALL_PACKAGES)
                // 不指定权限则自动获取清单中的危险权限
                .permission(pemission)
                .request(new OnPermission() {
                    @Override
                    public void hasPermission(List<String> granted, boolean isAll) {
                        if (isAll) {
                                permissionCallBack.GetPermissionFinish("1");
                        }else {
                            RequestPermission(currentActivity,pemission,permissionCallBack);
                        }
                    }

                    @Override
                    public void noPermission(List<String> denied, boolean quick) {
                        if(quick) {
                            //如果是被永久拒绝就跳转到应用权限系统设置页面 : 应该由上层决定
                            //gotoPermissionSettings(currentActivity);
                            permissionCallBack.GetPermissionFinish("0");
                        }else {
                            RequestPermission(currentActivity,pemission,permissionCallBack);
                        }
                    }
                });
    }
}
