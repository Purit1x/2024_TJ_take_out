//定制请求的实例
 
//导入axios  npm install axios
import axios from 'axios';
import { toast } from '@/utils/util';

//定义一个变量,记录公共的前缀  ,  baseURL
const baseURL = '/api';
const instance = axios.create({baseURL})
 
 
//添加响应拦截器
// instance.interceptors.response.use(
//     result=>{
//         return result.data;
//     },
//     err=>{
//         alert('服务异常');
//         return Promise.reject(err);//异步的状态转化成失败的状态
//     }
// )

// 添加响应拦截器
instance.interceptors.response.use(function (response) {
    // 对响应数据做点什么
    return response.data.data;
}, function (error) {
    
    // 对响应错误做点什么
    const msg = error.response.data.msg || "请求失败"

    toast(msg, 'error')

    return Promise.reject(error);
});
 
export default instance;