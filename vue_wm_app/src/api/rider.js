//导入request.js请求工具
import request from '@/utils/request.js'
import axios from 'axios'  
// 设置基本URL，这里使用你后端的地址  
const BASE_URL = 'http://localhost:5079/api'; 

// 骑手注册服务  
export const riderRegisterService = async (riderData) => {  
    try {  
        const response = await axios.post(`${BASE_URL}/Rider/register`, riderData);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }  
}
//提供调用登录接口的函数
export const riderLoginService = async(riderData) => {
    try {  
        const response = await axios.post(`${BASE_URL}/Rider/login`, riderData); 
        return response.data;
    } catch (error) {  
        throw error;   
    }  
}
export const riderInfo = async(id) => {  //获取骑手信息
    try {  
        const response = await axios.get(`${BASE_URL}/Rider/riderSearch?riderId=${id}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const updateRider = async(data) => {  //更新用户信息
    try {  
        const response = await axios.put(`${BASE_URL}/Rider/riderEdit`, data);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    } 
}
export const walletRecharge=async(id,addMoney) => {  //充值
    try {  
        const response = await axios.put(`${BASE_URL}/Rider/recharge?riderId=${id}&addMoney=${addMoney}`);  
        return response.data; // 返回后端返回的数据
    } catch (error) {  
        throw error;   
    }
}
