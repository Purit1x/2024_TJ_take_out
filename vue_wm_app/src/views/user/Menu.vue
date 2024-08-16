<script setup>
import { ref,onMounted, inject,watch,computed } from 'vue';
import { useRouter } from 'vue-router'
import {searchDishes} from '@/api/merchant';
//import{formatTime} from '@/views/user/UserPersonal.vue'
const router = useRouter();
const MerchantId=ref(null);  //商户id
const MerchantInfo=ref(null);  //商户信息
const dishes=ref([]);  //菜品列表
const showDishes=ref([]); //显示的菜品列表
const searchQuery = ref('');  // 搜索关键字
const merchantsInfo=inject('merchantsInfo');
onMounted (async() => {
  const storedMerchantsInfo = localStorage.getItem('MerchantInfo');  //长效保存
  if (storedMerchantsInfo) {  
    merchantsInfo.value = JSON.parse(storedMerchantsInfo);  
  }  
  MerchantId.value=router.currentRoute.value.params.id;
  MerchantInfo.value=merchantsInfo.value[MerchantId.value-1];
  if(MerchantInfo &&MerchantInfo.value.timeforOpenBusiness && MerchantInfo.value.timeforCloseBusiness){
  MerchantInfo.value.timeforOpenBusiness=formatTime(MerchantInfo.value.timeforOpenBusiness);  //时间转换
  MerchantInfo.value.timeforCloseBusiness=formatTime(MerchantInfo.value.timeforCloseBusiness);
  }
  console.log(merchantsInfo.value);
  const data = await searchDishes(MerchantId.value);  
  if (data) {  
    dishes.value = data.data; // 假设后端返回的菜品数据是一个数组
    dishes.value.filter(dish => dish.dishInventory > 0);  //过滤出库存大于0的菜品
    showDishes.value=dishes.value;
  }  
  console.log(dishes.value);
});
const gobackHome = () => {
  router.push('/user-home');
}
// 观察 merchantsInfo 的变化，并保存到本地存储  
watch(merchantsInfo, (newValue) => {  
  localStorage.setItem('MerchantInfo', JSON.stringify(newValue));  
});  
const formatTime=(seconds)=> {  
  const hours = Math.floor(seconds / 3600);  
  const minutes = Math.floor((seconds % 3600) / 60);  
  const secs = seconds % 60;  
  return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;  
}  
// 计算属性：搜索过滤  
const filteredDishes = computed(() => {  
  if (!searchQuery.value) {  
    return dishes.value; // 如果没有搜索关键字，返回所有菜品  
  }  
  return dishes.value.filter(dish => {  
    const nameMatch = dish.dishName.toLowerCase().includes(searchQuery.value.toLowerCase());  
    const categoryMatch = dish.dishCategory.toLowerCase().includes(searchQuery.value.toLowerCase());  
    return nameMatch || categoryMatch; // 根据名字或类别进行匹配  
  });  
});  
// 搜索函数  
const handleSearch = () => {  
  showDishes.value = filteredDishes.value; // 更新显示的菜品列表为过滤结果  
}  
</script>

<template>
    <div v-if="MerchantInfo && MerchantInfo.merchantName">
      {{MerchantInfo.merchantName}}的菜单
      <button @click="gobackHome()">&nbsp;&nbsp;返回</button>
      <div>地址：{{MerchantInfo.merchantAddress}}</div>
      <div>营业时间：{{ MerchantInfo.timeforOpenBusiness }} - {{ MerchantInfo.timeforCloseBusiness }}</div>
      <div>联系电话：{{MerchantInfo.contact}}</div>
      <div>
        <div>
          <input type="text" v-model="searchQuery" placeholder="搜索菜品或类别" v-on:keyup.enter="handleSearch()"/> 
          <button @click="handleSearch()">搜索</button>
        </div>
        <ul>
          <li v-for="dish in showDishes" :key="dish.dishId">
            <img :src="dish.imageUrl" alt="菜品图片" style="width: 50px; height: 50px;">
            {{dish.dishName}}：{{dish.dishPrice}}元 &nbsp;&nbsp;{{dish.dishCategory}}
            <button>下单</button>
            <button>加入购物车</button>
          </li>
      </ul>
      </div>
    </div>
</template>