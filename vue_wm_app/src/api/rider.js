//导入request.js请求工具
import request from '@/utils/request.js'
import { tryOnScopeDispose } from '@vueuse/core';
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
export const walletRecharge=async(id,addMoney) => {  //提现
    try {  
        const response = await axios.put(`${BASE_URL}/Rider/recharge?riderId=${id}&addMoney=${addMoney}`);  
        return response.data; // 返回后端返回的数据
    } catch (error) {  
        throw error;   
    }
}
export const stIdSearch=async(id) => {  //通过id查询站点
    try {  
        const response = await axios.get(`${BASE_URL}/Rider/StIdSearch?riderId=${id}`);  
        return response.data; // 返回后端返回的数据
    } catch (error) {  
        throw error;   
    }
}
export const getPaidOrders = async (id) => {  // 通过骑手ID获取本站点所有可接订单
    try {
        const response = await axios.get(`${BASE_URL}/Rider/getPaidOrders?riderId=${id}`);
        return response.data;
    }
    catch (error) {
        throw error;
    }
}
export const getReceivedOrders = async (id) => {  // 获取指定骑手已接订单
    try {
        const response = await axios.get(`${BASE_URL}/Rider/getReceivedOrders?riderId=${id}`);
        return response.data;
    }
    catch (error) {
        throw error;
    }
}

export const getDeliveredOrdersCountandAverageRating = async (id) => {  // 获取指定骑手已接订单数量与评价
    try {
        const response = await axios.get(`${BASE_URL}/Rider/getDeliveredOrdersCountandAverageRating?riderId=${id}`);
        return response.data;
    }
    catch (error) {
        throw error;
    }
}

export const receiveOrder = async (data) => {  // 骑手接单
    try {
        const response = await axios.put(`${BASE_URL}/Rider/receiveOrder`, data);
        return response.data;
    }
    catch (error) {
        throw error;
    }
} 
export const getRiderPrice = async (id) => {
    try {
        const response = await axios.get(`${BASE_URL}/Rider/getRiderPrice?orderId=${id}`);
        // console.log('配送费',response.data.data);
        return response.data.data;  // 返回配送费的数值（单位：元）
    }
    catch (error) {
        throw error;
    }
}
export const getOrdersWithinThisMonth = async (id) => {
    try {
        const response = await axios.get(`${BASE_URL}/Rider/getOrdersWithinThisMonth?riderId=${id}`);
        return response.data.data;  // 返回本月内订单列表, 若为空则返回0
    }
    catch (error) {
        throw error;
    }
}
export const getFinishedOrders = async (id) => {
    try {
        const response = await axios.get(`${BASE_URL}/Rider/getFinishedOrders?riderId=${id}`);
        return response.data.data;
    }
    catch (error) {
        throw error;
    }
}

export const walletWithdraw= async(id,withdrawMoney) => {  //提现
    try {  
        const response = await axios.put(`${BASE_URL}/Rider/withdraw?riderId=${id}&withdrawMoney=${withdrawMoney}`);  
        return response.data; // 返回后端返回的数据
    } catch (error) {  
        throw error;   
    }
}

