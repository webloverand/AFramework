package com.qianxi.afnative.Tool.ActivityResult;

import android.app.Activity;
import android.content.Intent;

public class ActResultRequest {

    private OnActResultEventDispatcherFragment fragment;
    private static final String TAG = "ActivityLauncher";

    public static ActResultRequest init(Activity activity) {
        return new ActResultRequest(activity);
    }

    private ActResultRequest(Activity activity) {

        fragment = getEventDispatchFragment(activity);
    }
    private OnActResultEventDispatcherFragment getEventDispatchFragment(Activity activity) {
        OnActResultEventDispatcherFragment routerFragment = findRouterFragment(activity);
        if (routerFragment == null) {
            routerFragment = OnActResultEventDispatcherFragment.newInstance();
            android.app.FragmentManager fragmentManager = activity.getFragmentManager();
            fragmentManager
                    .beginTransaction()
                    .add(routerFragment, TAG)
                    .commitAllowingStateLoss();
            fragmentManager.executePendingTransactions();
        }
        return routerFragment;
    }
    private OnActResultEventDispatcherFragment findRouterFragment(Activity activity) {
        return (OnActResultEventDispatcherFragment) activity.getFragmentManager().findFragmentByTag(TAG);
    }

    public void startForResult(Intent intent, ACTRequestCallback callback) {
        fragment.startForResult(intent, callback);
    }

    public interface ACTRequestCallback {

        void onActivityResult(int resultCode, Intent data);
    }
}
