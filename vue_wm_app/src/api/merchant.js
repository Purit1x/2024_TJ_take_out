//导入request.js请求工具
import request from '@/utils/request.js'
import axios from 'axios'  
import { ElMessage } from 'element-plus';
// 设置基本URL，这里使用你后端的地址  
const BASE_URL = 'http://localhost:5079/api'; 

// 用户注册服务  
export const merchantRegisterService = async (merchantData) => {  
    try {  
        const response = await axios.post(`${BASE_URL}/Merchant/register`, merchantData);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }  
}
//提供调用登录接口的函数
export const merchantLoginService = async(loginData) => {
    try {  
        const response = await axios.post(`${BASE_URL}/Merchant/login`, loginData); 
        return response.data;
    } catch (error) {  
        throw error;   
    }  
}

export const updateMerchant = async(data) => {
    try {  
        const response = await axios.put(`${BASE_URL}/Merchant/merchantEdit`, data);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    } 
}

export const merchantInfo = async(id) => {
    try {  
        const response = await axios.get(`${BASE_URL}/Merchant/merchantSearch?MerchantId=${id}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const walletRecharge=async(id,addMoney) => {
    try {  
        const response = await axios.put(`${BASE_URL}/Merchant/recharge?merchantId=${id}&addMoney=${addMoney}`);  
        return response.data; // 返回后端返回的数据
    } catch (error) {  
        throw error;   
    }
}
export const walletWithdraw=async(id,withdrawMoney) => {
    try {  
        const response = await axios.put(`${BASE_URL}/Merchant/withdraw?merchantId=${id}&withdrawMoney=${withdrawMoney}`);  
        return response.data; // 返回后端返回的数据
    } catch (error) {  
        throw error;   
    }
}
export const searchDishes = async(id) => {
    try {  
        const response = await axios.get(`http://localhost:5079/api/Merchant/dishSearch?merchantId=${id}`);
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const getAllStationInfos=async() => {
    try {  
        const response = await axios.get(`${BASE_URL}/Merchant/GetStations`);  
        const stationsInfo = response.data.data; // 获取站点数据   
        // 创建一个数组来保存包含 StationId 和对应的经纬度信息  
        const stationsWithCoordinates = [];  

        // 遍历每个站点，调用高德地图 API 获取经纬度  
        for (const station of stationsInfo) {  
            const address = encodeURIComponent(station.stationAddress); // 对地址进行 URL 编码  
            const geoResponse = await axios.get(`https://restapi.amap.com/v3/geocode/geo?address=${address}&output=JSON&key=873a2f8c732d6cf57df12bc8bef9572e`);  

            // 从高德地图 API 响应中提取经纬度  
            const location = geoResponse.data.geocodes && geoResponse.data.geocodes.length > 0  
                ? geoResponse.data.geocodes[0].location.split(',')  
                : [null, null]; // 如果没有获取到经纬度，返回 null  

            // 将 StationId 和经纬度（经度、纬度）一起推入结果数组  
            stationsWithCoordinates.push({  
                StationId: station.stationId,  
                Longitude: location[0], // 经度  
                Latitude: location[1]    // 纬度  
            });  
        }
        return stationsWithCoordinates; // 返回包含 StationId 和经纬度的数组  
    } catch (error) {  
        throw error;   
    }
}
export const assignStationToMerchant=async(address) => {
    try {  
        // 获取所有站点的经纬度信息  
        const stationsWithCoordinates = await getAllStationInfos();  

        // 获取目标地址的经纬度  
        const encodedAddress = encodeURIComponent(address); // 对地址进行 URL 编码  
        const geoResponse = await axios.get(`https://restapi.amap.com/v3/geocode/geo?address=${encodedAddress}&output=JSON&key=873a2f8c732d6cf57df12bc8bef9572e`); // 替换为您的高德地图 API Key  

        const targetLocation = geoResponse.data.geocodes && geoResponse.data.geocodes.length > 0  
            ? geoResponse.data.geocodes[0].location.split(',')  
            : [null, null]; // 如果没有获取到经纬度，返回 null  

        if (targetLocation[0] === null) return null; // 如果没有找到目标地址的经纬度，返回 null  

        const targetLongitude = parseFloat(targetLocation[0]);  
        const targetLatitude = parseFloat(targetLocation[1]);  

        let nearestStationId = null;  
        let minDistance = Infinity;  

        // 计算每个站点与目标地址之间的距离  
        for (const station of stationsWithCoordinates) {  
            const stationLongitude = parseFloat(station.Longitude);  
            const stationLatitude = parseFloat(station.Latitude);  

            // 使用 Haversine 公式计算距离（单位：千米）  
            const distance = haversineDistance(targetLatitude, targetLongitude, stationLatitude, stationLongitude);  

            // 如果当前站点距离更近，则更新最近站点 ID 和最小距离  
            if (distance < minDistance) {  
                minDistance = distance;  
                nearestStationId = station.StationId;  
            }  
        }  
        return nearestStationId; // 返回最近站点的 ID  
    } catch (error) {  
        throw error; // 抛出错误  
    }  
}
export const getDistanceBetweenAddresses = async (address1, address2) => {  
    try {  
        // 对地址进行 URL 编码  
        const encodedAddress1 = encodeURIComponent(address1);  
        const encodedAddress2 = encodeURIComponent(address2);  

        // 获取两个地址的经纬度  
        const geoResponse1 = await axios.get(`https://restapi.amap.com/v3/geocode/geo?address=${encodedAddress1}&output=JSON&key=873a2f8c732d6cf57df12bc8bef9572e`);  
        const geoResponse2 = await axios.get(`https://restapi.amap.com/v3/geocode/geo?address=${encodedAddress2}&output=JSON&key=873a2f8c732d6cf57df12bc8bef9572e`);  

        const location1 = geoResponse1.data.geocodes && geoResponse1.data.geocodes.length > 0  
            ? geoResponse1.data.geocodes[0].location.split(',')  
            : [null, null]; // 如果没有获取到经纬度，返回 null  

        const location2 = geoResponse2.data.geocodes && geoResponse2.data.geocodes.length > 0  
            ? geoResponse2.data.geocodes[0].location.split(',')  
            : [null, null]; // 如果没有获取到经纬度，返回 null  

        // 如果任一地址的经纬度未找到，则返回 null  
        if (location1[0] === null || location2[0] === null) return null;  

        const lat1 = parseFloat(location1[1]); // 纬度  
        const lon1 = parseFloat(location1[0]); // 经度  
        const lat2 = parseFloat(location2[1]); // 纬度  
        const lon2 = parseFloat(location2[0]); // 经度  

        // 使用 Haversine 公式计算距离（单位：千米）  
        const distanceInKm = haversineDistance(lat1, lon1, lat2, lon2);  
        return distanceInKm.toFixed(1); // 转换为小数点后一位
    } catch (error) {  
        throw error; // 抛出错误  
    }  
};  
// Haversine 公式计算两点距离（单位：千米）  
const haversineDistance = (lat1, lon1, lat2, lon2) => {  
    const R = 6371; // 地球半径，单位：千米  
    const dLat = degreesToRadians(lat2 - lat1);  
    const dLon = degreesToRadians(lon2 - lon1);  
    const a =  
        Math.sin(dLat / 2) * Math.sin(dLat / 2) +  
        Math.cos(degreesToRadians(lat1)) * Math.cos(degreesToRadians(lat2)) *  
        Math.sin(dLon / 2) * Math.sin(dLon / 2);  
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));  
    return R * c; // 返回距离，单位：千米  
};  
// 辅助函数：将度转化为弧度  
const degreesToRadians = (degrees) => {  
    if (typeof degrees !== 'number') {  
        throw new Error('Input must be a number');  
    }  
    return degrees * (Math.PI / 180);  
};  
export const EditMerchantStation=async(data) => {
    try {  
        const response = await axios.put(`${BASE_URL}/Merchant/editMerchantStation`, data);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const AssignStation=async(data) => {
    try {  
        const response = await axios.post(`${BASE_URL}/Merchant/assignStation`, data);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {   
        throw error;   
    }
}

export const CreateSpecialOffer=async(offer) => {    //创建满减服务
    try {  
        const response = await axios.post(`${BASE_URL}/Merchant/specialOfferCreate`, offer);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {   
        throw error;   
    }
}

export const EditSpecialOffer=async(offer) => {    //修改满减服务
    try {  
        const response = await axios.put(`${BASE_URL}/Merchant/specialOfferEdit`, offer);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {   
        throw error;   
    }
}

export const DeleteSpecialOffer = async (Id) => {    // 删除满减服务
    try {
        const response = await axios.delete(`${BASE_URL}/Merchant/deleteSpecialOffer`, {
            params: {   // 使用 params 来传递查询字符串参数
                offerId: Id
            },
            headers: {
                'Content-Type': 'application/json'  // 这个在 DELETE 请求中通常是不需要的，可以去掉
            }
        });
        return response.data; // 返回后端返回的数据
    } catch (error) {
        throw error;
    }
};

export const GetSpecialOffer=async(merchantId) => {    //获取商家提供的满减服务
    try {  
        const response = await axios.get(`${BASE_URL}/Merchant/specialOfferGet?merchantId=${merchantId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const getDishInfo = async(merchantId,dishId) => {
    try {  
        const response = await axios.get(`${BASE_URL}/Merchant/getDishInfo?merchantId=${merchantId}&dishId=${dishId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const GetMultiSpecialOffer=async(merchantIds) => {    //获取商家提供的满减服务
    try {
        // Create query string for multiple merchant IDs
        const queryString = merchantIds.map(id => `merchantIds=${id}`).join('&');
        
        // Make the GET request with the formatted query string
        const response = await axios.get(`${BASE_URL}/Merchant/multiSpecialOfferGet?${queryString}`);
        
        return response.data; // Return the data from the response
    } catch (error) {
        throw error;
    }
}
export const getOrdersToHandle = async(merchantId) => {
    try {  
        const response = await axios.get(`${BASE_URL}/Merchant/getOrdersToHandle?merchantId=${merchantId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const deletePaidOrder=async(orderId) => {
    try {  
        const response = await axios.delete(`${BASE_URL}/Merchant/deletePaidOrder?orderId=${orderId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const getMerAddrByOrderId = async (orderId) => {
    try {
        const response = await axios.get(`${BASE_URL}/Merchant/getMerAddrByOrderId?orderId=${orderId}`);
        return response.data;
    } catch (error) {
        throw error;
    }
}
export const deliverOrder = async (data) => {
    try{
        const response = await axios.put(`${BASE_URL}/Merchant/deliverOrder`, data);
        ElMessage.success('订单已送达');
        return response.data;
    }catch(error){
        throw error;
    }
}

export const getMerOrdersWithinThisMonth=async(id)=>{
    try{
        const response =await axios.get(`${BASE_URL}/Merchant/getMerOrdersWithinThisMonth?merchantId=${id}`);
        return response.data.data;//返回本月订单列表，空则返回0
    }
    catch(error){
        throw error;
    }
}

export const getMerOrdersWithinThisDay=async(id)=>{
    try{
        const response =await axios.get(`${BASE_URL}/Merchant/getMerOrdersWithinThisDay?merchantId=${id}`);
        return response.data.data;//返回本日订单列表，空则返回0
    }
    catch(error){
        throw error;
    }
}
export const getMerPrice=async(id)=>{
    try{
        const response =await axios.get(`${BASE_URL}/Merchant/getMerPrice?orderId=${id}`);
        console.log('销售盈利',response.data.data);
        return response.data.data;
    }
    catch (error) {
        throw error;
    }
}

export const getMerAvgRating=async(id)=>{
    try{
        const response=await axios.get(`${BASE_URL}/Merchant/getMerAvgRating?merchantId=${id}`);
        return response.data.data;//返回商家评分，
    }
    catch(error){
        throw error;
    }
}


export const getFinishedMerOrders=async(id)=>{
    try{
        const response=await axios.get(`${BASE_URL}/Merchant/getFinishedMerOrders?merchantId=${id}`);
        return response.data.data;
    }
    catch(error){
        throw error;
    }
}

