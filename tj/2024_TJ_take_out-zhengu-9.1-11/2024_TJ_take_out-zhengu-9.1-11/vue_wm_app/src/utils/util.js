import { ElNotification, ElMessageBox } from 'element-plus';

// 提示框
export function toast(message, type = 'success', dangerouslyUseHTMLString = false) {
    ElNotification({
        message: message,
        type: type,
        dangerouslyUseHTMLString: dangerouslyUseHTMLString,
        duration: 3000,
    })
}