package com.qianxi.afnative.Function;

import android.content.Context;
import android.os.Build;
import android.os.LocaleList;

import com.qianxi.afnative.Tool.FileOperate.UniversalID;

import java.util.Locale;
/*
获取系统信息脚本,比如UUID,国家
 */
public class SystemInfo {
    public String getCountry()
    {
       return getSysPreferredLocale().getCountry();
    }
    /**
     * 获取系统首选语言
     * 注意：该方法获取的是用户实际设置的不经API调整的系统首选语言
     * @return
     */
    public Locale getSysPreferredLocale() {
        Locale locale;
        //7.0以下直接获取系统默认语言
        if (Build.VERSION.SDK_INT < 24) {
            // 等同于context.getResources().getConfiguration().locale;
            locale = Locale.getDefault();
            // 7.0以上获取系统首选语言
        } else {
            /*
             * 以下两种方法等价，都是获取经API调整过的系统语言列表（可能与用户实际设置的不同）
             * 1.context.getResources().getConfiguration().getLocales()
             * 2.LocaleList.getAdjustedDefault()
             */
            // 获取用户实际设置的语言列表
            locale = LocaleList.getDefault().get(0);
        }
        return locale;
    }
    //获取设备唯一UUID
    public String getUniversalID(Context context) {
        return UniversalID.getUniversalID(context);
    }

}
