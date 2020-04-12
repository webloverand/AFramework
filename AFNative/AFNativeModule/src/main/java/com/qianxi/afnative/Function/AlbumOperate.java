package com.qianxi.afnative.Function;

import android.app.Activity;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.os.Handler;
import android.provider.MediaStore;

import com.qianxi.afnative.AFNativeInterface;
import com.qianxi.afnative.Tool.ActivityResult.ActResultRequest;

import java.io.File;

/*
相册功能封装
 */
public class AlbumOperate {
    //打开相册
    public void OpenAlbum(final Handler mainHandle, final Activity activity) {
        mainHandle.post(new Runnable() {
            @Override
            public void run() {
                Intent intent = new Intent(Intent.ACTION_PICK, null);
                intent.setDataAndType(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, "image/*");
                // 启动Activity
                ActResultRequest.init(activity)
                        .startForResult(intent, new ActResultRequest.ACTRequestCallback() {
                            @Override
                            public void onActivityResult(int resultCode, Intent data) {
                                // 处理回调信息
                                if (resultCode ==  activity.RESULT_OK) {
                                    // 从相册返回的数据
                                    if (data != null) {
                                        // 得到图片的全路径
                                        Uri uri = data.getData(); //uri转换成file
                                        uri = data.getData();
                                        //----------
                                        String[] arr = {MediaStore.Images.Media.DATA};
                                        Cursor cursor = activity.getContentResolver().query(uri, arr, null, null, null);
                                        int img_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
                                        cursor.moveToFirst();
                                        String img_path = cursor.getString(img_index);
                                        File file = new File(img_path);
                                        String path = file.getAbsolutePath();
                                        AFNativeInterface.SendToUnityMsg("AlbumPicturePath~"+path);
                                    }
                                    else
                                    {
                                        AFNativeInterface.SendToUnityMsg("AlbumPicturePath~");
                                    }
                                } else if (resultCode ==  activity.RESULT_CANCELED) {
                                    AFNativeInterface.SendToUnityMsg("AlbumPicturePath~");
                                }
                            }
                        });
            }
        });
    }
    //刷新相册
    public void ScanAlbum(Activity activity,String filePath) {
        Intent scanIntent = new Intent(Intent.ACTION_MEDIA_SCANNER_SCAN_FILE);
        scanIntent.setData(Uri.fromFile(new File(filePath)));
        activity.sendBroadcast(scanIntent);
        AFNativeInterface.SendToUnityMsg("SavePhotoOrVideo~1~" + filePath);
    }
}

